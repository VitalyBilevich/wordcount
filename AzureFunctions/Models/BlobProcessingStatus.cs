namespace AzureFunctions.Models
{
    public class BlobProcessingStatus
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Session { get; set; }

        public bool InProcessing { get; set; }        

        public DateTime Timestamp { get; set; }
    }
}
