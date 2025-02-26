namespace AzureFunctions.Сonstants
{
    public class ConstBlobContainer
    {
        public const string FileProcessingBlobContainerName = "files";

        public const string BlobTriggerPath = "files/{name}";

        public const string BlobTriggerConnection = "AzureWebJobsStorage";
    }
}
