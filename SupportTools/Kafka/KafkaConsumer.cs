using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;

namespace WebMarket.OrderService.SupportTools.Kafka
{
    public class KafkaConsumer : BackgroundService
    {
        private ILogger<KafkaConsumer> _logger;
        public KafkaConsumer(ILogger<KafkaConsumer> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumerConfig = new ConsumerConfig()
            {
                BootstrapServers = "localhost:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest,// If no record of the previous commited offsets? consumer will start there
                SecurityProtocol = SecurityProtocol.Plaintext,
                GroupId = "jopa-group", //identify multiple consumers that belong to a single group
                EnableAutoCommit = true, // Commit message - consumer doesn't want to receive it again
            };
            return Task.Run(() =>
            {
                try
                {
                    using (var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build()) //If value is complex? we neet to specify SetValueDeserializer
                    {
                        consumer.Subscribe("test_topic");
                        _logger.LogInformation("Begin listenning kafka");
                        while (!stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogInformation("cycle1");
                            var result = consumer.Consume(stoppingToken);
                            if (result == null)
                                continue;

                            _logger.LogInformation($"Consumed: {result.Message.Key} {result.Message.Value} {result.Offset}");
                        }
                        consumer.Close();
                    }
                }
                catch (OperationCanceledException) { }
                finally
                {

                }
            });
           
            
        }

    }
}
