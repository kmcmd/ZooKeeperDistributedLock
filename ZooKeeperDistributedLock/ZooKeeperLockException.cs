using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZooKeeperDistributedLock
{
    public class ZooKeeperLockException : Exception
    {
        public ZooKeeperLockException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }
}
