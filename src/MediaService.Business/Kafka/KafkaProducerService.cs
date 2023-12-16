using Confluent.Kafka;
using MediaService.Business.Abstractions.Kafka;
using MediaService.Shared.DTO.Kafka;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace MediaService.Business.Kafka
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IConfiguration _configuration;
        private readonly IProducer<string, string> _producer;

        public KafkaProducerService(IConfiguration configuration)
        {
            _configuration = configuration;
            _producer = new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
            }).Build();
        }

        public async Task ProduceNotificationAsync(SendNotificationKafka sendNotificationKafka)
        {
            var topic = _configuration["Kafka:SendNotification:Topic"];
            var value = JsonSerializer.Serialize(sendNotificationKafka);

            await _producer.ProduceAsync(topic, new Message<string, string> { Value = value });
        }
    }
}