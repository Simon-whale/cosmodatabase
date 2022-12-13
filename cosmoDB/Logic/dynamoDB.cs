using System.Net;
using cosmoDB.Models;
using Microsoft.Azure.Cosmos;

namespace cosmoDB.Logic;

public class DynamoDb : CosmoContext<Family>
{
    private readonly string _databaseId = "db";
    private readonly string _containerId = "items";

    public async Task GetStartedDemoAsync()
    {
        Family andersenFamily = new Family
        {
            Id = "Andersen.1",
            LastName = "Andersen",
            Parents = new[]
            {
                new Parent { FirstName = "Thomas" },
                new Parent { FirstName = "Mary Kay" }
            },
            Children = new[]
            {
                new Child
                {
                    FirstName = "Henriette Thaulow",
                    Gender = "female",
                    Grade = 5,
                    Pets = new[]
                    {
                        new Pet { GivenName = "Fluffy" }
                    }
                }
            },
            Address = new Address { State = "WA", County = "King", City = "Seattle" },
            IsRegistered = false
        };

        Family wakefieldFamily = new Family
        {
            Id = "Wakefield.7",
            LastName = "Wakefield",
            Parents = new[]
            {
                new Parent { FamilyName = "Wakefield", FirstName = "Robin" },
                new Parent { FamilyName = "Miller", FirstName = "Ben" }
            },
            Children = new[]
            {
                new Child
                {
                    FamilyName = "Merriam",
                    FirstName = "Jesse",
                    Gender = "female",
                    Grade = 8,
                    Pets = new[]
                    {
                        new Pet { GivenName = "Goofy" },
                        new Pet { GivenName = "Shadow" }
                    }
                },
                new Child
                {
                    FamilyName = "Miller",
                    FirstName = "Lisa",
                    Gender = "female",
                    Grade = 1
                }
            },
            Address = new Address { State = "NY", County = "Manhattan", City = "NY" },
            IsRegistered = true
        };

        var status = await CreateDatabaseAsync(_databaseId);
        await CreateContainerAsync(_containerId, "/LastName");
        if (status == HttpStatusCode.Created)
        {
            await ScaleContainer(100);    
        }
        
        await AddItemToContainerAsync(andersenFamily, new PartitionKey(andersenFamily.LastName));
        await AddItemToContainerAsync(wakefieldFamily, new PartitionKey(wakefieldFamily.LastName));

        var item = await queryResults<Family>("Wakefield.7", "Wakefield");
        item.IsRegistered = true;
        item.Children[0].Grade = 6;

        await UpdateRecord(item.Id, new PartitionKey(item.LastName), item);
        Console.WriteLine("Update Completed....");

        var output = await QueryItemsAsync<Family>("SELECT * FROM c");
        output.ForEach(p => Console.WriteLine(p.LastName));
        Console.WriteLine(output.ToString());

        var response = DeleteItem<Family>("Wakefield.7", new PartitionKey("Wakefield"));
        await RemoveDatabase();
        _cosmosClient.Dispose();
    }
}