namespace AzureFunctions.Models
{
    public class SessionProcessingResult
    {
        public string Id { get; set; }

        public string SessionId { get; set; }

        public bool FilesInProcessing { get; set; }

        public Dictionary<string, int> WordCounts { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
