using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static org.apache.zookeeper.Watcher;

namespace ZooKeeperDistributedLock;

public class LockWatcher : Watcher
{
    private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

    public override Task process(WatchedEvent @event)
    {
        if (@event.get_Type() == Event.EventType.NodeDeleted)
        {
            _tcs.TrySetResult(true);
        }
        return Task.CompletedTask;
    }

    public Task WaitAsync() => _tcs.Task;
}
