namespace ERDM.Infrastructure.Configuration
{
    public class RedisConfig
    {
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
        public int DefaultExpirationMinutes { get; set; } = 10;
        public bool AbortOnConnectFail { get; set; } = false;
        public int ConnectTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;

    }
}
