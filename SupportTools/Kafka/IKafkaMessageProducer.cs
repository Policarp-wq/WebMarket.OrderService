
namespace WebMarket.OrderService.SupportTools.Kafka
{
    public interface IKafkaMessageProducer
    {
        Task ProduceMessage(string topic, string key, string message);
    }
}