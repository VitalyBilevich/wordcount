using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using AzureFunctions.Сonstants;
using Azure.Storage.Blobs;
using TextProcessor;

namespace AzureFunctions.Functions
{
    public class ProcessSmallBlobFunction
    {
        private readonly ILogger<ProcessSmallBlobFunction> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IWordCounter _wordCounter;

        public ProcessSmallBlobFunction(ILogger<ProcessSmallBlobFunction> logger, BlobServiceClient blobServiceClient, IWordCounter wordCounter)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
            _wordCounter = wordCounter;
        }

        [Function(ConstFunction.ProcessSmallBlobFunction)]
        public async Task<Dictionary<string, int>> ProcessSmallBlob([ActivityTrigger] string blobName)
        {
            _logger.LogInformation($"Processing small blob: {blobName}");

            var containerClient = _blobServiceClient.GetBlobContainerClient(ConstBlobContainer.FileProcessingBlobContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            var fileContent = await blobClient.DownloadContentAsync();

            return _wordCounter.CountWordsDFAConcurrent(fileContent.Value.Content.ToString());
        }
    }
}
