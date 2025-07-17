using Microsoft.Extensions.FileSystemGlobbing;
using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZooKeeperDistributedLock
{
    public class ZooKeeperDistributedLock : IDistributedLock
    {
        private readonly ZooKeeper _zooKeeper;
        private readonly string _lockRootPath;
        private readonly string _lockName;
        private string _lockNodePath;
        private bool _isAcquired;

        public ZooKeeperDistributedLock(ZooKeeper zooKeeper, string lockRootPath, string lockName)
        {
            _zooKeeper = zooKeeper ?? throw new ArgumentNullException(nameof(zooKeeper));
            _lockRootPath = lockRootPath ?? throw new ArgumentNullException(nameof(lockRootPath));
            _lockName = lockName ?? throw new ArgumentNullException(nameof(lockName));
        }

        public async Task<bool> TryAcquireAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await EnsureRootPathAsync();

                // Create ephemeral sequential node
                _lockNodePath = await _zooKeeper.createAsync(
                    $"{_lockRootPath}/{_lockName}_",
                    Array.Empty<byte>(),
                    ZooDefs.Ids.OPEN_ACL_UNSAFE,
                    CreateMode.EPHEMERAL_SEQUENTIAL
                );

                while (!cancellationToken.IsCancellationRequested)
                {
                    // Get all nodes and sort
                    var children = (await _zooKeeper.getChildrenAsync(_lockRootPath, false)).Children
                        .Select(c => $"{_lockRootPath}/{c}")
                        .OrderBy(c => c)
                        .ToList();

                    // Ensure our node is in the list
                    int myIndex = children.IndexOf(_lockNodePath);
                    if (myIndex == -1)
                    {
                        throw new ZooKeeperLockException("Lock node not found; session may have expired.");
                    }

                    // Check if our node is the first in the sequence
                    if (myIndex == 0)
                    {
                        _isAcquired = true;
                        return true;
                    }

                    // Watch the previous node
                    var previousNodePath = children[myIndex - 1];
                    var watcher = new LockWatcher();
                    var previousNode = await _zooKeeper.existsAsync(previousNodePath, watcher);

                    if (previousNode == null)
                    {
                        // Previous node already deleted, recheck immediately
                        continue;
                    }

                    await Task.WhenAny(watcher.WaitAsync(), Task.Delay(-1, cancellationToken));
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await ReleaseAsync(cancellationToken);
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new ZooKeeperLockException("Failed to acquire lock.", ex);
            }
        }

        public async Task ReleaseAsync(CancellationToken cancellationToken = default)
        {
            if (!_isAcquired || string.IsNullOrEmpty(_lockNodePath))
                return;

            try
            {
                await _zooKeeper.deleteAsync(_lockNodePath, -1);
                _isAcquired = false;
                _lockNodePath = null;
            }
            catch (Exception ex)
            {
                throw new ZooKeeperLockException("Failed to release lock.", ex);
            }
        }

        public void Dispose()
        {
            if (_isAcquired)
            {
                ReleaseAsync().GetAwaiter().GetResult();
            }
        }

        private async Task EnsureRootPathAsync()
        {
            if (await _zooKeeper.existsAsync(_lockRootPath, false) == null)
            {
                try
                {
                    await _zooKeeper.createAsync(
                        _lockRootPath,
                        Array.Empty<byte>(),
                        ZooDefs.Ids.OPEN_ACL_UNSAFE,
                        CreateMode.PERSISTENT
                    );
                }
                catch (KeeperException.NodeExistsException) { }
            }
        }
    }
}