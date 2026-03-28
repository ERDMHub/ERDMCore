using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ERDM.Infrastructure.Health
{
    public class MongoHealthCheck : IHealthCheck
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoHealthCheck> _logger;

        public MongoHealthCheck(IMongoDatabase database, ILogger<MongoHealthCheck> logger)
        {
            _database = database;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var result = await _database.RunCommandAsync<BsonDocument>(
                    new BsonDocument("ping", 1),
                    cancellationToken: cancellationToken);
                var duration = DateTime.UtcNow - startTime;

                var data = new Dictionary<string, object>
                {
                    { "DatabaseName", _database.DatabaseNamespace.DatabaseName },
                    { "ResponseTimeMs", duration.TotalMilliseconds }
                };

                return HealthCheckResult.Healthy(
                    $"MongoDB is healthy. Response time: {duration.TotalMilliseconds:F2}ms",
                    data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MongoDB health check failed");
                return HealthCheckResult.Unhealthy("MongoDB is unhealthy", ex);
            }
        }
    }
}
