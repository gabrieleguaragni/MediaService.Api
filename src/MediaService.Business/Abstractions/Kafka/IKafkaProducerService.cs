using MediaService.Shared.DTO.Kafka;

namespace MediaService.Business.Abstractions.Kafka
{
    public interface IKafkaProducerService
    {
        public Task ProduceNotificationAsync(SendNotificationKafka sendNotificationKafka);
    }
}