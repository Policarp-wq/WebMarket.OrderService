using Confluent.Kafka;

namespace WebMarket.OrderService.SupportTools.Kafka
{
    public class KafkaMessageProducer : IKafkaMessageProducer
    {
        private readonly ILogger<KafkaMessageProducer> _logger;
        private readonly IProducer<string, string> _mainProducer;
        public KafkaMessageProducer(ILogger<KafkaMessageProducer> logger, IProducer<string, string> mainProducer)
        {
            _logger = logger;
            _mainProducer = mainProducer;
        }

        public async Task ProduceMessage(string topic, string key, string message)
        {
            _logger.LogDebug("Sending message to kafka");
            await _mainProducer.ProduceAsync(topic, new Message<string, string>()
            {
                Key = key,
                Value = message
            });
        }
    }
}
