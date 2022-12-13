using Microsoft.Azure.Cosmos;
using cosmoDB.Logic;
using Microsoft.Extensions.Hosting;


try
{
    Console.WriteLine("Beginning operations...\n");
    using var db = new DynamoDb();
    await db.GetStartedDemoAsync();
}
catch (CosmosException ex)
{
    Console.WriteLine("{0} error occurred: {1}", ex.StatusCode, ex);
}
catch (Exception e)
{
    Console.WriteLine("Error: {0}", e);
}
finally
{
    Console.WriteLine("End of demo, press any key to exit.");
    Console.ReadKey();
}