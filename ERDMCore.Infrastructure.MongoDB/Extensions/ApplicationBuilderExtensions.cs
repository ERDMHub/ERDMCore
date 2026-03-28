
using ERDMCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace ERDMCore.Infrastructure.MongoDB.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseInfrastructureMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<RequestTimingMiddleware>();
            app.UseMiddleware<RateLimitingMiddleware>();

            return app;
        }

        public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder app)
        {
            app.UseMiddleware<AuthenticationMiddleware>();
            return app;
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                await next();
            });
            return app;
        }
    }
}
