using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using AzureFunctions.Models;
using AzureFunctions.Сonstants;


namespace AzureFunctions.Functions
{
    public class DurableOrchestratorFunction
    {
        private readonly ILogger<DurableOrchestratorFunction> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly Container _cosmosContainer;

        public DurableOrchestratorFunction(ILogger<DurableOrchestratorFunction> logger,
            BlobServiceClient blobServiceClient,
            Func<string, Container> cosmosContainerResolver)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
            _cosmosContainer = cosmosContainerResolver(СonstCosmosDB.FileProcessingContainerName);
        }

        [Function(ConstFunction.DurableOrchestratorFunction)]
        public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var blobMetaData = context.GetInput<BlobMetadata>();
            if (blobMetaData == null)
            {
                _logger.LogInformation("No blob metadata found");
                return;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(ConstBlobContainer.FileProcessingBlobContainerName);

            Task<Dictionary<string, int>> task;
            const long largeFileSizeThreshold = 10 * 1024 * 1024; // 10 MB         

            if (blobMetaData.Size >= largeFileSizeThreshold)
            {
                task = context.CallActivityAsync<Dictionary<string, int>>(ConstFunction.ProcessLargeBlobFunction, blobMetaData.Name);
            }
            else
            {
                task = context.CallActivityAsync<Dictionary<string, int>>(ConstFunction.ProcessSmallBlobFunction, blobMetaData.Name);
            }

            var result = await task;
            if (result.Count == 0)
                return;

            await context.CallActivityAsync(ConstFunction.StoreResultsFunction, new StoreResultsData { WordCounts = result, SessionId = blobMetaData.SessionId });

            await context.CallActivityAsync(ConstFunction.MarkFileAsProcessedFunction, blobMetaData);
        }




    }
}