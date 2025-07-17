using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZooKeeperDistributedLock;

public interface IDistributedLock : IDisposable
{
    Task<bool> TryAcquireAsync(CancellationToken cancellationToken = default);
    Task ReleaseAsync(CancellationToken cancellationToken = default);
}
