namespace ERDM.Infrastructure.Configuration
{
    public class SignalRSettings
    {
        public bool EnableDetailedErrors { get; set; } = false;
        public int KeepAliveIntervalSeconds { get; set; } = 15;
        public int ClientTimeoutIntervalSeconds { get; set; } = 30;
        public string RedisBackplane { get; set; }
    }
}
