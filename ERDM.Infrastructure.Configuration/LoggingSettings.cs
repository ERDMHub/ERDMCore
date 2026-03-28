namespace ERDM.Infrastructure.Configuration
{
    public class LoggingSettings
    {
        public string Level { get; set; }
        public bool EnableConsole { get; set; }
        public bool EnableFile { get; set; }
        public string FilePath { get; set; }
        public bool EnableElasticSearch { get; set; }
        public string ElasticSearchUrl { get; set; }
    }
}
