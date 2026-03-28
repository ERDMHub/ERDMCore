using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ERDMCore.Infrastructure.Logging
{
    public class LogContextEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogContextEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Dictionary<string, object> Enrich(HttpContext context = null)
        {
            var httpContext = context ?? _httpContextAccessor.HttpContext;
            var properties = new Dictionary<string, object>();

            if (httpContext != null)
            {
                // Correlation ID
                var correlationId = httpContext.Items["CorrelationId"]?.ToString()
                    ?? httpContext.TraceIdentifier;
                properties["CorrelationId"] = correlationId;

                // Request information
                properties["RequestPath"] = httpContext.Request.Path.ToString();
                properties["RequestMethod"] = httpContext.Request.Method;
                properties["UserAgent"] = httpContext.Request.Headers["User-Agent"].ToString();
                properties["IpAddress"] = httpContext.Connection.RemoteIpAddress?.ToString();

                // User information if authenticated
                if (httpContext.User?.Identity?.IsAuthenticated == true)
                {
                    properties["UserId"] = httpContext.User.FindFirst("sub")?.Value
                        ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    properties["UserName"] = httpContext.User.Identity.Name;
                    properties["UserEmail"] = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;

                    var roles = httpContext.User.FindAll(ClaimTypes.Role).Select(r => r.Value);
                    properties["UserRoles"] = string.Join(",", roles);
                }
            }

            // Environment information
            properties["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            properties["MachineName"] = Environment.MachineName;
            properties["ProcessId"] = Environment.ProcessId;
            properties["Timestamp"] = DateTime.UtcNow;

            return properties;
        }
    }
}
