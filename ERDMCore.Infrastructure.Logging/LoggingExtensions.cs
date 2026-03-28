using ERDMCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Security.Claims;

namespace ERDMCore.Infrastructure.Logging
{
    public static class LoggingExtensions
    {
        public static IServiceCollection AddCustomLogging(this IServiceCollection services, string applicationName)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("MongoDB.Driver", LogEventLevel.Warning)
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development")
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("ProcessId", Environment.ProcessId)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: $"logs/{applicationName}-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    fileSizeLimitBytes: 10485760, // 10 MB
                    rollOnFileSizeLimit: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            // Register logging services
            services.AddSingleton<ILoggerService, SerilogLoggerService>();
            services.AddSingleton<LogContextEnricher>();
            services.AddHttpContextAccessor();

            // Add Serilog to the logging pipeline
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(dispose: true);
            });

            return services;
        }

        public static IApplicationBuilder UseCustomLogging(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                // Add correlation ID to logs
                var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                    ?? context.TraceIdentifier;

                // Push properties directly
                using (LogContext.PushProperty("CorrelationId", correlationId))
                using (LogContext.PushProperty("RequestPath", context.Request.Path))
                using (LogContext.PushProperty("RequestMethod", context.Request.Method))
                using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].ToString()))
                using (LogContext.PushProperty("IpAddress", context.Connection.RemoteIpAddress?.ToString()))
                {
                    await next();
                }
            });

            return app;
        }

        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
                    diagnosticContext.Set("RequestPath", httpContext.Request.Path);
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                    diagnosticContext.Set("IpAddress", httpContext.Connection.RemoteIpAddress?.ToString());

                    if (httpContext.User?.Identity?.IsAuthenticated == true)
                    {
                        diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
                        diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
                    }
                };
            });

            return app;
        }

        // Alternative: Custom middleware for more control
        public static IApplicationBuilder UseCustomRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }

   
}