
using Confluent.Kafka;

namespace WebMarket.OrderService.SupportTools.Kafka
{
    public interface IKafkaMessageProducer
    {
        Task<DeliveryResult<string, string>> ProduceMessage(string topic, string key, string message);
    }
}