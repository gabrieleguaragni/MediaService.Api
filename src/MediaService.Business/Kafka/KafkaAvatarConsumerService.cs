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
    public class KafkaAvatarConsumerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<KafkaAvatarConsumerService> _logger;
        private readonly IKafkaProducerService _kafkaProducerService;

        public KafkaAvatarConsumerService(
            IConfiguration configuration, 
            IServiceProvider serviceProvider, 
            ILogger<KafkaAvatarConsumerService> logger,
            IKafkaProducerService kafkaProducerService
            )
        {
            _configuration = configuration;
            _consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = _configuration["Kafka:SendAvatar:GroupId"],
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

            var topic = _configuration["Kafka:SendAvatar:Topic"];
            _consumer.Subscribe(topic);
            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                SendAvatarKafka? sendAvatarKafka = JsonSerializer.Deserialize<SendAvatarKafka>(consumeResult.Message.Value, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                });

                if (sendAvatarKafka != null)
                {
                    try
                    {
                        await _imageService.UploadAvatar(sendAvatarKafka.File, sendAvatarKafka.FileExtension, sendAvatarKafka.FileName);
                        _consumer.Commit(consumeResult);

                        await _kafkaProducerService.ProduceNotificationAsync(new SendNotificationKafka()
                        {
                            IDUser = sendAvatarKafka.IDUser,
                            Message = "Avatar updated successfully",
                            Type = NotificationType.Info
                        });
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error occurred in KafkaAvatarConsumerService");
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
