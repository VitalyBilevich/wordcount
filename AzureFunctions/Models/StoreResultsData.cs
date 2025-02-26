namespace AzureFunctions.Models
{
    public class StoreResultsData
    {
        public Dictionary<string, int> WordCounts { get; set; }

        public string SessionId { get; set; }
    }
}
