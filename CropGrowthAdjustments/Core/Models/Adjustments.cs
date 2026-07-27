using System.Collections.Generic;
using Newtonsoft.Json;
using StardewModdingAPI;

namespace CropGrowthAdjustments.Core.Models;

public class Adjustments
{
    public List<CropAdjustment> CropAdjustments { get; set; } = new();

    [JsonIgnore] public IContentPack ContentPack { get; set; } = null!;
}