using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ZooKeeperDistributedLock
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddZooKeeperDistributedLock(this IServiceCollection services, IConfiguration configuration)
        {
            //services.Configure<ZooKeeperLockOptions>(configuration.GetSection("ZooKeeperLock"));
            services.AddSingleton<ZooKeeperLockFactory>();
            return services;
        }
    }
}
