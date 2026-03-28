namespace ERDM.Infrastructure.Configuration
{
    public class PostgresConfig
    {
        public string ConnectionString { get; set; }
        public int CommandTimeout { get; set; } = 30;
        public int MaxPoolSize { get; set; } = 100;
        public bool EnableRetry { get; set; } = true;
    }
}
