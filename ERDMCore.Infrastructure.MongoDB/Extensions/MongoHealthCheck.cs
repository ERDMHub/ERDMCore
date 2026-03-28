using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;


namespace ERDMCore.Infrastructure.MongoDB.Extensions
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

        public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("MongoDB is healthy");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MongoDB health check failed");
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("MongoDB is unhealthy", ex);
            }
        }
    }
}
