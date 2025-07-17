using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using Microsoft.Extensions.Options;
using org.apache.zookeeper;

namespace ZooKeeperDistributedLock
{
    public class ZooKeeperLockFactory
    {
        private readonly ZooKeeperLockOptions _options;
        private readonly ZooKeeper _zooKeeper;

        public ZooKeeperLockFactory(IOptions<ZooKeeperLockOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _zooKeeper = new ZooKeeper(
                _options.ConnectionString,
                _options.ConnectionTimeoutMs,
                null // No default watcher needed
            );
        }

        public IDistributedLock CreateLock(string lockName)
        {
            if (string.IsNullOrWhiteSpace(lockName))
                throw new ArgumentException("Lock name cannot be empty.", nameof(lockName));

            return new ZooKeeperDistributedLock(_zooKeeper, _options.LockRootPath, lockName);
        }

        public void Dispose()
        {
            _zooKeeper?.closeAsync().GetAwaiter().GetResult();
        }
    }
}