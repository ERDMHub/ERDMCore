namespace ERDM.Infrastructure.Configuration
{
    public class KafkaConfig
    {
        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }
        public bool EnableAutoCommit { get; set; } = true;
        public int AutoCommitIntervalMs { get; set; } = 5000;
    }
}
