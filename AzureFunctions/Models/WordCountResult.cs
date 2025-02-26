namespace AzureFunctions.Models
{
    public class WordCountResult
    {
        public string Id { get; set; }

        public string Word { get; set; }

        public string Session { get; set; }

        public int Count { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
