using ERDMCore.Infrastructure.MongoDB.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ERDM.Infrastructure.Health
{
    public static class HealthCheckBuilder
    {
        public static IHealthChecksBuilder AddCustomHealthChecks(this IServiceCollection services)
        {
            return services.AddHealthChecks()
                .AddCheck<MongoHealthCheck>("MongoDB", tags: new[] { "database", "critical" })
                .AddCheck<RedisHealthCheck>("Redis", tags: new[] { "cache", "critical" })
                .AddCheck<RabbitMQHealthCheck>("RabbitMQ", tags: new[] { "message-bus", "critical" })
                .AddCheck("Memory", new MemoryHealthCheck(512), tags: new[] { "system" })
                .AddCheck("Disk", new DiskHealthCheck(1024), tags: new[] { "system" });
        }
    }
}
