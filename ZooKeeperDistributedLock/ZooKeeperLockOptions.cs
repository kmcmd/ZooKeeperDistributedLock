using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZooKeeperDistributedLock;

public class ZooKeeperLockOptions
{
    public string ConnectionString { get; set; } = "localhost:2181";
    public int ConnectionTimeoutMs { get; set; } = 15000;
    public string LockRootPath { get; set; } = "/locks";
}