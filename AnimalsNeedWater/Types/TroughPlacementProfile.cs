using System.Collections.Generic;
using Newtonsoft.Json;

namespace AnimalsNeedWater.Types
{
    public class TroughPlacementProfile
    {
        [JsonProperty("modUniqueId")]
        public string ModUniqueId { get; set; }
        [JsonProperty("targetBuildings")]
        public List<string> TargetBuildings { get; set; }
        [JsonProperty("placement")]
        public List<PlacementItem> Placement { get; set; }

        public PlacementItem GetPlacementForBuildingName(string buildingName) =>
            Placement[TargetBuildings.ConvertAll(s => s.ToLower()).IndexOf(buildingName.ToLower())];
        public bool BuildingHasWateringSystem(string buildingName) =>
            GetPlacementForBuildingName(buildingName)?.WateringSystem != null;
    }

    public class PlacementItem
    {
        [JsonProperty("troughTiles")]
        public List<TroughTile> TroughTiles;
        [JsonProperty("wateringSystem")]
        public WateringSystemTile WateringSystem;
    }
    
    public class TroughTile
    {
        [JsonProperty("tileX")]
        public int TileX { get; set; }
        [JsonProperty("tileY")]
        public int TileY { get; set; }
        [JsonProperty("layer")]
        public string Layer { get; set; }
        [JsonProperty("emptyIndex")]
        public int EmptyIndex { get; set; }
        [JsonProperty("fullIndex")]
        public int FullIndex { get; set; }
    }

    public class WateringSystemTile
    {
        [JsonProperty("tileX")]
        public int TileX { get; set; }
        [JsonProperty("tileY")]
        public int TileY { get; set; }
        [JsonProperty("layer")]
        public string Layer { get; set; }
        [JsonProperty("systemIndex")]
        public int SystemIndex { get; set; }
        [JsonProperty("tilesToRemove")]
        public List<SimplifiedTile> TilesToRemove { get; set; }
    }

    public class SimplifiedTile
    {
        [JsonProperty("tileX")]
        public int TileX { get; set; }
        [JsonProperty("tileY")]
        public int TileY { get; set; }
        [JsonProperty("layer")]
        public string Layer { get; set; }
    }
}