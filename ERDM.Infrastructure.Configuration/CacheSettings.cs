namespace ERDM.Infrastructure.Configuration
{
    public class CacheSettings
    {
        public RedisConfig Redis { get; set; }
        public MemoryCacheConfig MemoryCache { get; set; }
    }
}
