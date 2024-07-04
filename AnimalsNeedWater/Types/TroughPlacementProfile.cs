using System.Collections.Generic;
using Newtonsoft.Json;

namespace AnimalsNeedWater.Types
{
    public class TroughPlacementProfile
    {
        public string modUniqueId;
        public List<string> targetBuildings;
        public List<TroughTile> barnTroughTiles;
        public List<TroughTile> barn2TroughTiles;
        public List<TroughTile> barn3TroughTiles;
        public WateringSystemTile barn3WateringSystem;
        public List<TroughTile> coopTroughTiles;
        public List<TroughTile> coop2TroughTiles;
        public List<TroughTile> coop3TroughTiles;
        public WateringSystemTile coop3WateringSystem;
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