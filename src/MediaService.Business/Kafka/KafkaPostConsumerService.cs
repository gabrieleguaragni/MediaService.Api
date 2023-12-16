using Confluent.Kafka;
using MediaService.Business.Abstractions.Kafka;
using MediaService.Business.Abstractions.Services;
using MediaService.Shared.DTO.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MediaService.Business.Kafka
{
    public class KafkaPostConsumerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<KafkaPostConsumerService> _logger;
        private readonly IKafkaProducerService _kafkaProducerService;

        public KafkaPostConsumerService(
            IConfiguration configuration, 
            IServiceProvider serviceProvider, 
            ILogger<KafkaPostConsumerService> logger,
            IKafkaProducerService kafkaProducerService
            )
        {
            _configuration = configuration;
            _consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = _configuration["Kafka:SendPost:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            }).Build();
            _serviceProvider = serviceProvider;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            using var scope = _serviceProvider.CreateScope();
            var _imageService = scope.ServiceProvider.GetRequiredService<IImageService>();

            var topic = _configuration["Kafka:SendPost:Topic"];
            _consumer.Subscribe(topic);
            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                SendPostKafka? sendPostKafka = JsonSerializer.Deserialize<SendPostKafka>(consumeResult.Message.Value, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                });

                if (sendPostKafka != null)
                {
                    try
                    {
                        await _imageService.UploadPost(sendPostKafka.File, sendPostKafka.FileExtension, sendPostKafka.FileName);
                        _consumer.Commit(consumeResult);

                        await _kafkaProducerService.ProduceNotificationAsync(new SendNotificationKafka()
                        {
                            IDUser = sendPostKafka.IDUser,
                            Message = "Post created successfully",
                            Type = NotificationType.Info
                        });
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error occurred in KafkaPostConsumerService");
                    }
                }
            }
        }

        public override void Dispose()
        {
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
