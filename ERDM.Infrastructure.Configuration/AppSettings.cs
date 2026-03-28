namespace ERDM.Infrastructure.Configuration
{
    public class AppSettings
    {
        public string ApplicationName { get; set; }
        public string Environment { get; set; }
        public string Version { get; set; }
        public CorsSettings Cors { get; set; }
        public LoggingSettings Logging { get; set; }
    }
}
