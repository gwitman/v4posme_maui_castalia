using Newtonsoft.Json;

namespace v4posme_maui.Models;

public class DtoMenssage
{
    [JsonProperty("error")]
    public bool Error { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }
}