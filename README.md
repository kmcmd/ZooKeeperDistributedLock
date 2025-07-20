# ZooKeeperDistributedLock

![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0-blueviolet) ![NuGet](https://img.shields.io/nuget/v/ZooKeeperDistributedLock?color=green) ![License](https://img.shields.io/github/license/kmcmd/ZooKeeperDistributedLock?color=blue)

A robust and easy-to-use distributed lock library for .NET, powered by Apache ZooKeeper. This library enables developers to synchronize access to shared resources across distributed systems with a simple, async-friendly API. Whether you're building microservices, distributed applications, or systems requiring high concurrency, `ZooKeeperDistributedLock` ensures reliable and fair locking.

## Installation

```bash
dotnet add package ZooKeeperDistributedLock
```

## Features

- **Asynchronous API**: Fully async/await compatible for modern .NET applications.
- **Dependency Injection**: Seamless integration with ASP.NET Core and other DI frameworks.
- **Robust Error Handling**: Gracefully handles ZooKeeper connection issues and session expirations.
- **Configurable**: Customize connection settings via `appsettings.json` or code.
- **Lightweight and Performant**: Built on ZooKeeper’s efficient ephemeral nodes for fast lock operations.
- **NuGet Ready**: Packaged for easy installation and use in any .NET project.

## Configuration

You can configure the library using either `appsettings.json` or directly in code.

### Option 1: Using `appsettings.json`

Add the following section to your `appsettings.json`:

```json
{
  "ZooKeeperLock": {
    "ConnectionString": "localhost:2181",
    "ConnectionTimeoutMs": 15000,
    "LockRootPath": "/locks"
  }
}
```
### Option 2: Programmatic ConfigurationConfigure
Configure the library directly in code:

```csharp
services.Configure<ZooKeeperLockOptions>(options =>
{
    options.ConnectionString = "localhost:2181";
    options.ConnectionTimeoutMs = 15000;
    options.LockRootPath = "/locks";
});
```
## Usage

The library provides a simple API for acquiring and releasing distributed locks. Below are examples demonstrating its use with dependency injection and standalone.

## Example 1: With Dependency Injection (ASP.NET Core)

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZooKeeperDistributedLock;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddZooKeeperDistributedLock(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Acquire and use a lock
        var factory = serviceProvider.GetService<ZooKeeperLockFactory>();
        using var distributedLock = factory.CreateLock("my-resource");

        if (await distributedLock.TryAcquireAsync())
        {
            try
            {
                Console.WriteLine("Lock acquired! Performing critical operation...");
                await Task.Delay(1000);
            }
            finally
            {
                await distributedLock.ReleaseAsync();
                Console.WriteLine("Lock released.");
            }
        }
        else
        {
            Console.WriteLine("Failed to acquire lock.");
        }
    }
}

```

## Handling Lock Contention
The library uses ZooKeeper’s sequential ephemeral nodes to ensure fair lock acquisition. If a lock is already held, TryAcquireAsync waits until the lock is released or the operation is canceled.


```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
if (await distributedLock.TryAcquireAsync(cts.Token))
{
    try
    {
        Console.WriteLine("Lock acquired after waiting.");
    }
    finally
    {
        await distributedLock.ReleaseAsync();
    }
}
else
{
    Console.WriteLine("Failed to acquire lock within timeout.");
}
```

## Contributing
Contributions are welcome! Please submit issues or pull requests to the GitHub repository.

## Support
For questions or support, open an issue on GitHub or reach out via kamalmansouri91[at]gmail.com Check out the documentation for more details.







