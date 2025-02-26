using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.DurableTask.Client;
using AzureFunctions.Models;
using AzureFunctions.Ñonstants;

namespace AzureFunctions.Functions
{
    public class BlobTriggerFunction
    {
        private readonly ILogger<BlobTriggerFunction> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly Container _cosmosContainer;

        public BlobTriggerFunction(ILogger<BlobTriggerFunction> logger,
            BlobServiceClient blobServiceClient,
            Func<string, Container> containerResolver
            )
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
            _cosmosContainer = containerResolver(ÑonstCosmosDB.FileProcessingContainerName);
        }

        [Function(ConstFunction.BlobTriggerFunction)]
        public async Task Run([BlobTrigger(ConstBlobContainer.BlobTriggerPath, Connection = ConstBlobContainer.BlobTriggerConnection)] string blobContent,
            string name,
            [DurableClient] DurableTaskClient client)
        {
            _logger.LogInformation($"Blob trigger function processed blob\n Name: {name}");

            var containerClient = _blobServiceClient.GetBlobContainerClient(ConstBlobContainer.FileProcessingBlobContainerName);
            var blobClient = containerClient.GetBlobClient(name);
            var properties = await containerClient.GetBlobClient(name).GetPropertiesAsync();
            var sessionId = properties.Value.Metadata.TryGetValue("session", out var session) ? session : "0";

            if (!await SetFileInProcessing(name, sessionId))
            {
                _logger.LogInformation($"Skipping {name}, because it already in progress.");
                return;
            }

            var blobMetadata = new BlobMetadata
            {
                Name = name,
                Size = properties.Value.ContentLength,
                SessionId = sessionId
            };

            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(ConstFunction.DurableOrchestratorFunction, blobMetadata);

            _logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }

        // Check Cosmos DB to see if the file is being processed, and if not, mark it as being processed.
        private async Task<bool> SetFileInProcessing(string blobName, string sessionId)
        {
            try
            {

                string sqlQuery = "SELECT * FROM c WHERE c.name = @name";
                var queryDefinition = new QueryDefinition(sqlQuery).WithParameter("@name", blobName);

                FeedIterator<BlobProcessingStatus> feedIterator = _cosmosContainer.GetItemQueryIterator<BlobProcessingStatus>(
                queryDefinition,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(sessionId)
                });


                if (feedIterator.HasMoreResults)
                {
                    FeedResponse<BlobProcessingStatus> response = await feedIterator.ReadNextAsync();

                    if (response.Count > 0)
                    {
                        var blobProcessingStatus = response.First();
                        if (blobProcessingStatus.InProcessing)
                        {
                            return false;
                        }
                        else
                        {
                            blobProcessingStatus.InProcessing = true;
                            blobProcessingStatus.Timestamp = DateTime.UtcNow;
                            await _cosmosContainer.UpsertItemAsync(blobProcessingStatus, new PartitionKey(sessionId));
                            return true;
                        }
                    }
                }

                await MarkNewFileAsProcessing(blobName, sessionId);
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // File doesn't exist, which means it's not being processed
                return false;
            }
        }

        private async Task MarkNewFileAsProcessing(string blobName, string sessionId)
        {
            var statusItem = new BlobProcessingStatus
            {
                Name = blobName,
                Id = Guid.NewGuid().ToString(),
                Session = sessionId,
                InProcessing = true,
                Timestamp = DateTime.UtcNow
            };

            // Create or update the file's status in Cosmos DB
            await _cosmosContainer.UpsertItemAsync(statusItem, new PartitionKey(sessionId));
        }
    }
}
