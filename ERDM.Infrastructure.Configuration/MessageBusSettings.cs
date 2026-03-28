
namespace ERDM.Infrastructure.Configuration
{
    public class MessageBusSettings
    {
        public RabbitMQConfig RabbitMQ { get; set; }
        public KafkaConfig Kafka { get; set; }
    }
}
