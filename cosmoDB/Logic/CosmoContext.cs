using System.Net;
using cosmoDB.Models;
using cosmoDB.Repository;
using Microsoft.Azure.Cosmos;

namespace cosmoDB.Logic;

public abstract class CosmoContext<T> : IDisposable where T : class
{
    internal readonly CosmosClient _cosmosClient;
    internal Database _database;
    internal Container _container;
    
    public CosmoContext()
    {
        //todo add to appsettings
        var endpointUri = "https://localhost:8081";
        var primaryKey =
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        _cosmosClient = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
    }

    public async Task<HttpStatusCode> CreateDatabaseAsync(string databaseId)
    {
        var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        _database = database.Database;
        
        return database.StatusCode;
    }

    public async Task CreateContainerAsync(string containerId, string partionKeyPath)
    {
        var container = await _database.CreateContainerIfNotExistsAsync(containerId, partionKeyPath, 400);
        _container = container.Container;
        if (container.StatusCode == HttpStatusCode.Created)
            Console.WriteLine("Created Container: {0}\n", _container.Id);
        else
            Console.WriteLine("Connecting to container {0}", _container.Id);
    }

    public async Task ScaleContainer(int scaleValue)
    {
        int? throughput = await _container.ReadThroughputAsync();
        if (throughput.HasValue)
        {
            Console.WriteLine("Current provisioned throughput : {0}\n", throughput.Value);
            int newThroughput = throughput.Value + scaleValue;
            
            await _container.ReplaceThroughputAsync(newThroughput);
            Console.WriteLine("New provisioned throughput : {0}\n", newThroughput);
        }
    }
    
    public async Task<List<T>> QueryItemsAsync<T>(string sqlQueryText)
    {
        Console.WriteLine("Running query: {0}\n", sqlQueryText);

        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
        FeedIterator<T> queryResultSetIterator = _container.GetItemQueryIterator<T>(queryDefinition);

        var data = new List<T>();

        while (queryResultSetIterator.HasMoreResults)
        {
            FeedResponse<T> currentResultSet = await queryResultSetIterator.ReadNextAsync();
            foreach (T items in currentResultSet)
            {
                data.Add(items);
            }
        }
        
        return data;
    }

    public async Task<T> queryResults<T>(string Id, string partitionKeyValue) 
    {
        Console.WriteLine("Getting an Item");
        ItemResponse<T> response = await _container.ReadItemAsync<T>(Id, new PartitionKey(partitionKeyValue));
        return response.Resource;
    }

    public async Task<bool> AddItemToContainerAsync<T>(T item, PartitionKey partitionKey)
    {
        try
        {
            var response = await _container.CreateItemAsync(item, partitionKey); 
            return true;
        }
        catch (CosmosException e)
        {
            //todo add better handling here
            return false;
        }
        
    }
    
    public async Task UpdateRecord<T>(string Id, PartitionKey partitionKey, T data) where T : class
    {
       await _container.ReplaceItemAsync(data, Id, partitionKey);
    }

    public async Task<bool> DeleteItem<T>(string Id, PartitionKey partitionKeyValue)
    {
        try
        {
            await _container.DeleteItemAsync<T>(Id, partitionKeyValue);
            return true;
        }
        catch (CosmosException e)
        {
            return false;
        }
    }

    public async Task RemoveDatabase()
    {
         await _database.DeleteAsync();   
    }
    
    public void Dispose() => _cosmosClient.Dispose();
}