using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ERDMCore.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();
        private readonly int _requestLimit = 100;
        private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1);

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(clientIp))
            {
                await _next(context);
                return;
            }

            var clientInfo = _clients.GetOrAdd(clientIp, new ClientRequestInfo());

            lock (clientInfo)
            {
                var now = DateTime.UtcNow;
                if (now - clientInfo.WindowStart > _timeWindow)
                {
                    clientInfo.WindowStart = now;
                    clientInfo.RequestCount = 0;
                }

                clientInfo.RequestCount++;

                if (clientInfo.RequestCount > _requestLimit)
                {
                    _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}", clientIp);
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.Headers["Retry-After"] = _timeWindow.TotalSeconds.ToString();
                    context.Response.Headers["X-RateLimit-Limit"] = _requestLimit.ToString();
                    context.Response.Headers["X-RateLimit-Remaining"] = "0";
                    context.Response.Headers["X-RateLimit-Reset"] = (clientInfo.WindowStart + _timeWindow).ToString();
                    return;
                }
            }

            await _next(context);
        }
    }
}
