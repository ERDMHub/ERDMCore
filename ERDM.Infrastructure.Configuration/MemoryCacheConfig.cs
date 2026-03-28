namespace ERDM.Infrastructure.Configuration
{
    public class MemoryCacheConfig
    {
        public int SizeLimit { get; set; } = 100;
        public int DefaultExpirationMinutes { get; set; } = 5;
        public int CompactionPercentage { get; set; } = 5;
    }
}
