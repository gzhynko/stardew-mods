using System.Collections.Generic;
using AnimalsNeedWater.Core.Models;
using Newtonsoft.Json;

namespace AnimalsNeedWater.Core.Content.Models;

public class WateringSystemTile
{
    [JsonProperty("tileX")]
    public int TileX { get; set; }
    [JsonProperty("tileY")]
    public int TileY { get; set; }
    [JsonProperty("layer")]
    public string? Layer { get; set; }
    [JsonProperty("systemIndex")]
    public int SystemIndex { get; set; }
    [JsonProperty("tilesToRemove")]
    public List<SimplifiedTile> TilesToRemove { get; set; } = new ();
}