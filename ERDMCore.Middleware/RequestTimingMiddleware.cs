using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ERDMCore.Middleware
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;

        public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            context.Items["StartTime"] = DateTime.UtcNow;

            await _next(context);

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            context.Response.Headers["X-Response-Time-ms"] = elapsedMs.ToString();

            if (elapsedMs > 1000)
            {
                _logger.LogWarning("Slow request: {Path} took {ElapsedMs}ms", context.Request.Path, elapsedMs);
            }
        }
    }
}
