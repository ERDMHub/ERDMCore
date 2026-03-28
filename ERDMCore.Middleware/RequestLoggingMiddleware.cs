using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;


namespace ERDMCore.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = Guid.NewGuid().ToString();

            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            // Log request
            var requestLog = await LogRequest(context);
            _logger.LogInformation("Request: {RequestLog}", requestLog);

            try
            {
                await _next(context);
                stopwatch.Stop();

                // Log response
                var responseLog = LogResponse(context, stopwatch.ElapsedMilliseconds);
                _logger.LogInformation("Response: {ResponseLog}", responseLog);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Request failed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        private async Task<string> LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();
            var body = await ReadBodyAsync(context.Request);

            return new
            {
                CorrelationId = context.Items["CorrelationId"],
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                Body = body,
                IpAddress = context.Connection.RemoteIpAddress?.ToString()
            }.ToString();
        }

        private string LogResponse(HttpContext context, long elapsedMs)
        {
            return new
            {
                CorrelationId = context.Items["CorrelationId"],
                StatusCode = context.Response.StatusCode,
                ElapsedMs = elapsedMs,
                Headers = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            }.ToString();
        }

        private async Task<string> ReadBodyAsync(HttpRequest request)
        {
            if (request.ContentLength > 0 && request.Body.CanRead)
            {
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
                return body.Length > 1000 ? body.Substring(0, 1000) + "..." : body;
            }
            return null;
        }
    }
}
