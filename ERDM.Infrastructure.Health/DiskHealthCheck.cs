using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ERDM.Infrastructure.Health
{
    public class DiskHealthCheck : IHealthCheck
    {
        private readonly long _thresholdMB;

        public DiskHealthCheck(long thresholdMB) => _thresholdMB = thresholdMB;

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(Directory.GetCurrentDirectory()));
            var freeSpaceMB = driveInfo.AvailableFreeSpace / (1024 * 1024);
            var status = freeSpaceMB > _thresholdMB ? HealthStatus.Healthy : HealthStatus.Degraded;

            return Task.FromResult(new HealthCheckResult(
                status,
                $"Free disk space: {freeSpaceMB} MB / {_thresholdMB} MB",
                data: new Dictionary<string, object> { ["FreeSpaceMB"] = freeSpaceMB }));
        }
    }
}
