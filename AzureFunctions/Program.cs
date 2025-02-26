using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Cosmos;
using TextProcessor;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(new BlobServiceClient(context.Configuration.GetValue<string>("AzureWebJobsStorage")));

        // Add Cosmos DB container to save file processing status and results. We'll need to add another one if we decide to store status and results in different containers.
        services.AddSingleton<Func<string, Container>>(serviceProvider => key =>
        {
            // Factory to create a container based on the key. For now, we only have one container.
            var cosmosClient = new CosmosClient(context.Configuration.GetValue<string>("CosmosDBConnectionString"), new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
            });
            if (key == "WordCounts")
            {
                return cosmosClient.GetContainer(context.Configuration.GetValue<string>("CosmosDataBaseId"), context.Configuration.GetValue<string>("CosmosContainerId"));
            }

            throw new InvalidOperationException("Invalid Cosmos DB container key");
        });

        services.AddScoped<IWordCounter, WordCounter>();
    })
    .Build();

await host.RunAsync();