using System.Collections.Generic;
using Newtonsoft.Json;

namespace AnimalsNeedWater.Core.Content.Models;

public class BundledPlacementProfile
{
    public string? RequiredModId { get; set; }
    public Dictionary<string, TroughPlacement>? Entries { get; set; }
}