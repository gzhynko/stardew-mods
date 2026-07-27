using Newtonsoft.Json;

namespace AnimalsNeedWater.Core.Content.Models;

public class TroughTile
{
    [JsonProperty("tileX")]
    public int TileX { get; set; }
    [JsonProperty("tileY")]
    public int TileY { get; set; }
    [JsonProperty("layer")]
    public string? Layer { get; set; }
    [JsonProperty("emptyIndex")]
    public int EmptyIndex { get; set; }
    [JsonProperty("fullIndex")]
    public int FullIndex { get; set; }
}
