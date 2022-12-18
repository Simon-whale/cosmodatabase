using System.Net;
using cosmoDB.Models;
using Microsoft.Azure.Cosmos;

namespace cosmoDB.Logic;

public class StockRepo : CosmoContext<Stocks>
{
    private readonly string _databaseId = "db";
    private readonly string _containerId = "stocks";

    public async Task StartDemoAsync()
    {
        var status = await CreateDatabaseAsync(_databaseId);
        await CreateContainerAsync(_containerId, "/Name");
        if (status == HttpStatusCode.Created)
        {
            await ScaleContainer(100);
        }

        for (int i = 0; i < 100; i++)
        {
            var newStock = new Stocks()
            {
                Id = i.ToString(),
                Name = $"Stock Name {i}",
                Item = "This is a new stock item",
                Quantity = i
            };

            Console.WriteLine(newStock.Name);
            await AddItemToContainerAsync(newStock, new PartitionKey(newStock.Name));
        }

        var output = await QueryItemsAsync<Stocks>("select * from c");
        Console.WriteLine(output.Count);
        output.ForEach(p => Console.WriteLine(p.ToString()));
        _cosmosClient.Dispose();
    }
}