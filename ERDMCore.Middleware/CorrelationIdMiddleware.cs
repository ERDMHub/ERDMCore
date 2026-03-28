using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace ERDMCore.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? context.Request.Headers["Correlation-Id"].FirstOrDefault()
                ?? Guid.NewGuid().ToString();

            context.TraceIdentifier = correlationId;
            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestPath"] = context.Request.Path,
                ["Method"] = context.Request.Method
            }))
            {
                await _next(context);
            }
        }
    }
}
