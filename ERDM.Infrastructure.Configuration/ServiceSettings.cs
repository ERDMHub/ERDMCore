
namespace ERDM.Infrastructure.Configuration
{
    public class ServiceSettings
    {
        public JwtSettings Jwt { get; set; }
        public SwaggerSettings Swagger { get; set; }
        public HangfireSettings Hangfire { get; set; }
        public SignalRSettings SignalR { get; set; }
    }
}
