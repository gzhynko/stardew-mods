using Newtonsoft.Json;

namespace AnimalsNeedWater.Core.Content.Models;

public class SimplifiedTile
{
    [JsonProperty("tileX")]
    public int TileX { get; set; }
    [JsonProperty("tileY")]
    public int TileY { get; set; }
    [JsonProperty("layer")]
    public string? Layer { get; set; }
}