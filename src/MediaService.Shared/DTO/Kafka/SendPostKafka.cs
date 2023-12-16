namespace MediaService.Shared.DTO.Kafka
{
    public class SendPostKafka
    {
        public long IDUser { get; set; }

        public string File { get; set; }

        public string FileExtension { get; set; }

        public string FileName { get; set; }
    }
}
