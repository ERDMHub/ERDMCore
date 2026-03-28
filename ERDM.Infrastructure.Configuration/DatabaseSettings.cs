
namespace ERDM.Infrastructure.Configuration
{
    public class DatabaseSettings
    {
        public MongoDbConfig MongoDB { get; set; }
        public PostgresConfig Postgres { get; set; }
    }
}
