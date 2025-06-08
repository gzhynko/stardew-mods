using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AnimalsNeedWater.Core.Models;

public class TroughPlacementConfiguration
{
    [JsonIgnore] public string ModId { get; set; } = null!;
    public Dictionary<string, BuildingTroughPlacement> Placement { get; set; } = null!;
}