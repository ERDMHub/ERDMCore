using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;


namespace ERDM.Infrastructure.Health
{
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisHealthCheck> _logger;

        public RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var database = _redis.GetDatabase();
                var startTime = DateTime.UtcNow;
                var ping = await database.PingAsync();
                var duration = DateTime.UtcNow - startTime;

                var data = new Dictionary<string, object>
                {
                    { "PingTimeMs", ping.TotalMilliseconds },
                    { "IsConnected", _redis.IsConnected },
                    { "Endpoint", _redis.Configuration }
                };

                return HealthCheckResult.Healthy(
                    $"Redis is healthy. Ping time: {ping.TotalMilliseconds:F2}ms",
                    data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis health check failed");
                return HealthCheckResult.Unhealthy("Redis is unhealthy", ex);
            }
        }
    }
}
