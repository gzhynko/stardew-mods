using System.Collections.Generic;
using AnimalsNeedWater.Core.Models;
using Newtonsoft.Json;

namespace AnimalsNeedWater.Core.Content.Models;

public class BuildingTroughPlacement
{
    [JsonProperty("troughTiles")]
    public List<TroughTile> TroughTiles = new ();
    [JsonProperty("wateringSystem")]
    public WateringSystemTile? WateringSystem;
}
