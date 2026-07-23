using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;

namespace CropGrowthAdjustments.Core.Models;

public class SpecialSprites
{
    public string? Season { get; set; }
    public string? Sprites { get; set; }
    public string? LocationsToIgnore { get; set; }
    
    [JsonIgnore]
    public Season ParsedSeason { get; set; }
    [JsonIgnore]
    public List<string> ParsedLocationsToIgnore { get; set; } = new();
    [JsonIgnore] 
    public Texture2D SpritesTexture { get; set; } = null!;
    [JsonIgnore]
    public string? GeneratedAssetName { get; set; }

}