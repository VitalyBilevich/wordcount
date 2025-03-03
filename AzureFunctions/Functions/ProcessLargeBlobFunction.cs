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

            var chunkSize = 1 * 1024 * 1024; // 1 MB
            var tasks = new List<Task<Dictionary<string, int>>>();

            await foreach (var chunk in DownloadBlobInChunks(blobClient, chunkSize))
            {
                tasks.Add(Task.Run(() => _wordCounter.CountWordsDFAConcurrent(chunk)));
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

        private async IAsyncEnumerable<string> DownloadBlobInChunks(BlobClient blobClient, int chunkSize)
        {
            var downloadInfo = await blobClient.DownloadAsync();
            using var stream = downloadInfo.Value.Content;
            using var reader = new StreamReader(stream);

            char[] buffer = new char[chunkSize];
            int bytesRead;
            while ((bytesRead = await reader.ReadAsync(buffer, 0, chunkSize)) > 0)
            {
                yield return new string(buffer, 0, bytesRead);
            }
        }
    }
}
