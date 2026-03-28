
namespace ERDM.Infrastructure.Configuration
{
    public class MongoDbConfig
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public int MinPoolSize { get; set; } = 10;
        public int MaxPoolSize { get; set; } = 100;
        public int ConnectionTimeoutSeconds { get; set; } = 30;
        public int SocketTimeoutSeconds { get; set; } = 60;
        public string WriteConcern { get; set; } = "majority";
        public bool JournalEnabled { get; set; } = true;
        public string ReadPreference { get; set; } = "Primary";
    }
}
