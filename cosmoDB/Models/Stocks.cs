using Newtonsoft.Json;

namespace cosmoDB.Models;

public class Stocks
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }
    public string Item { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }

    public override string ToString()
    {
        return $"{Id} - {Item}: Quantity {Quantity}";
    }
}

