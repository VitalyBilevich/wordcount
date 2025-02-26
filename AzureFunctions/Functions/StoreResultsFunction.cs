using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using AzureFunctions.Models;
using AzureFunctions.Сonstants;

namespace AzureFunctions.Functions
{
    public class StoreResultsFunction
    {
        private readonly ILogger<StoreResultsFunction> _logger;        
        private readonly Container _cosmosContainer;

        public StoreResultsFunction(ILogger<StoreResultsFunction> logger,            
            Func<string, Container> cosmosContainerResolver)
        {
            _logger = logger;          
            _cosmosContainer = cosmosContainerResolver(СonstCosmosDB.FileProcessingContainerName);
        }

        [Function(ConstFunction.StoreResultsFunction)]
        public async Task StoreResults([ActivityTrigger] StoreResultsData storeResultsData)
        {
            if (storeResultsData == null || storeResultsData.WordCounts == null || storeResultsData.WordCounts.Count == 0 || string.IsNullOrWhiteSpace(storeResultsData.SessionId))
                return;

            var tasks = storeResultsData.WordCounts.Select(async kvp => await UpdateWordCountResult(new WordCountResult { Id = kvp.Key, Word = kvp.Key, Count = kvp.Value, Session = storeResultsData.SessionId }));
            await Task.WhenAll(tasks);
            _logger.LogInformation("Word counts stored successfully.");
        }

        // Thread-safe, concurrent update logic
        public async Task UpdateWordCountResult(WordCountResult wordCountResult)
        {
            var attempt = 0;
            var maxRetries = 100;
            var success = false;
            while (attempt < maxRetries && !success)
            {
                attempt++;
                try
                {
                    WordCountResult currentWordCountResult;
                    try
                    {
                        var response = await _cosmosContainer.ReadItemAsync<WordCountResult>(wordCountResult.Id, new PartitionKey(wordCountResult.Session));
                        currentWordCountResult = response.Resource;
                        currentWordCountResult.Count += wordCountResult.Count;
                        currentWordCountResult.Timestamp = DateTime.UtcNow;

                        // Use the ETag to ensure that the document hasn't been modified by another operation
                        var requestOptions = new ItemRequestOptions
                        {
                            IfMatchEtag = response.ETag
                        };

                        await _cosmosContainer.ReplaceItemAsync(currentWordCountResult, currentWordCountResult.Id, new PartitionKey(currentWordCountResult.Session), requestOptions);
                    }
                    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        currentWordCountResult = new WordCountResult { Id = wordCountResult.Word, Word = wordCountResult.Word, Session = wordCountResult.Session, Count = wordCountResult.Count };
                        await _cosmosContainer.UpsertItemAsync(currentWordCountResult, new PartitionKey(currentWordCountResult.Session));
                    }

                    success = true;
                }
                catch (CosmosException cosmosException) when (cosmosException.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    _logger.LogInformation($"Retry {attempt}: Document was modified by another operation.");
                    await Task.Delay(100);
                }
                catch (CosmosException cosmosException)
                {
                    _logger.LogInformation($"Error occurred: {cosmosException.Message}");
                    break;
                }
            }
        }
    }
}
