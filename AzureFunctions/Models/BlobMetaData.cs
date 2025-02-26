namespace AzureFunctions.Models
{
    public class BlobMetadata
    {
        public string Name { get; set; }

        public long Size { get; set; }

        public string SessionId { get; set; }
    }
}
