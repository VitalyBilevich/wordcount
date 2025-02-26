using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using AzureFunctions.Сonstants;
using TextProcessor;


namespace AzureFunctions.Functions
{
    public class ProcessLargeBlobFunction
    {
        private readonly ILogger<ProcessLargeBlobFunction> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IWordCounter _wordCounter;

        public ProcessLargeBlobFunction(ILogger<ProcessLargeBlobFunction> logger,
            BlobServiceClient blobServiceClient, IWordCounter wordCounter)            
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
            _wordCounter = wordCounter;
        }

        [Function(ConstFunction.ProcessLargeBlobFunction)]
        public async Task<Dictionary<string, int>> ProcessLargeBlob([ActivityTrigger] string blobName)
        {
            _logger.LogInformation($"Processing large blob: {blobName}");

            var containerClient = _blobServiceClient.GetBlobContainerClient(ConstBlobContainer.FileProcessingBlobContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            var content = await blobClient.DownloadContentAsync();
            var blobText = content.Value.Content.ToString();
            var chunkSize = 1 * 1024 * 1024; // 1 MB

            var tasks = new List<Task<Dictionary<string, int>>>();
            for (int i = 0; i < blobText.Length; i += chunkSize)
            {
                var chunk = blobText.Substring(i, chunkSize);
                tasks.Add(Task.Run(() => _wordCounter.CountWords(chunk)));
            }

            var mapResults = await Task.WhenAll(tasks);
            var finalResult = new Dictionary<string, int>();
            foreach (var result in mapResults)
            {
                foreach (var kvp in result)
                {
                    if (finalResult.ContainsKey(kvp.Key))
                        finalResult[kvp.Key] += kvp.Value;
                    else
                        finalResult[kvp.Key] = kvp.Value;
                }
            }

            return finalResult;
        }       
    }
}
