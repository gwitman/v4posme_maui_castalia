using Newtonsoft.Json;

namespace v4posme_maui.Models;

public class Detalle
{
    [JsonProperty("quantity")]
    public decimal Quantity { get; set; }
    
    [JsonProperty("description")]
    public string? Description { get; set; }
    
    [JsonProperty("price")]
    public decimal Price { get; set; }
    [JsonProperty("url_product")]
    public string? UrlProduct { get; set; }
}