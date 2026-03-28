
namespace ERDM.Infrastructure.Configuration
{
    public class RabbitMQConfig
    {
        public string HostName { get; set; }
        public int Port { get; set; } = 5672;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; } = "/";
        public int RetryCount { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
    }
}
