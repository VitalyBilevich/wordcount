using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using AzureFunctions.Models;
using AzureFunctions.Сonstants;


namespace AzureFunctions.Functions
{
    public class MarkFileAsProcessedFunction
    {
        private readonly ILogger<MarkFileAsProcessedFunction> _logger;
        private readonly Container _cosmosContainer;

        public MarkFileAsProcessedFunction(ILogger<MarkFileAsProcessedFunction> logger,
            Func<string, Container> cosmosContainerResolver)
        {
            _logger = logger;
            _cosmosContainer = cosmosContainerResolver(СonstCosmosDB.FileProcessingContainerName);
        }

        [Function(ConstFunction.MarkFileAsProcessedFunction)]
        public async Task Run([ActivityTrigger] BlobMetadata blobMetadata)
        {
            string sqlQuery = "SELECT * FROM c WHERE c.name = @name";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQuery).WithParameter("@name", blobMetadata.Name);

            FeedIterator<BlobProcessingStatus> feedIterator = _cosmosContainer.GetItemQueryIterator<BlobProcessingStatus>(
            queryDefinition,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(blobMetadata.SessionId)
            });


            if (feedIterator.HasMoreResults)
            {
                FeedResponse<BlobProcessingStatus> response = await feedIterator.ReadNextAsync();

                if (response.Count > 0)
                {
                    var process = response.First();
                    process.InProcessing = false;
                    await _cosmosContainer.UpsertItemAsync(process, new PartitionKey(blobMetadata.SessionId));

                }
            }

            _logger.LogInformation($"Marked {blobMetadata.Name} as processed");
        }
    }
}
