using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ERDM.Infrastructure.Health
{
    public class MemoryHealthCheck : IHealthCheck
    {
        private readonly long _thresholdMB;

        public MemoryHealthCheck(long thresholdMB)
            => _thresholdMB = thresholdMB;

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var usedMemory = GC.GetTotalMemory(false) / (1024 * 1024);

            var status = usedMemory < _thresholdMB
                ? HealthStatus.Healthy
                : HealthStatus.Degraded;

            var result = new HealthCheckResult(
                status,
                description: $"Memory usage: {usedMemory} MB / {_thresholdMB} MB",
                data: new Dictionary<string, object>
                {
                    ["MemoryUsedMB"] = usedMemory,
                    ["ThresholdMB"] = _thresholdMB
                });

            return Task.FromResult(result);
        }
    }
}