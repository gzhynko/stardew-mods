using System.Collections.Generic;
using Newtonsoft.Json;
using StardewValley;

namespace CropGrowthAdjustments.Core.Models;

public class CropAdjustment
{
    public string? CropProduceName { get; set; }
    public string CropProduceItemId { get; set; } = "-1";
    /// <summary> comma-separated seasons where crops grow but do not produce </summary>
    public string? SeasonsToGrowIn { get; set; }
    /// <summary> comma-separated seasons where crops grow and produce </summary>
    public string? SeasonsToProduceIn { get; set; }
    /// <summary> comma-separated seasons where planted crops pause growth entirely (optional) </summary>
    public string? SeasonsToHibernateIn { get; set; }
    public string? LocationsWithDefaultSeasonBehavior { get; set; }
    public List<SpecialSprites> SpecialSpritesForSeasons { get; set; } = new();

    [JsonIgnore] 
    public List<Season> ParsedSeasonsToGrowIn { get; set; } = new();

    [JsonIgnore] 
    public List<Season> ParsedSeasonsToProduceIn { get; set; } = new();

    [JsonIgnore] 
    public List<Season> ParsedSeasonsToHibernateIn { get; set; } = new();

    [JsonIgnore] 
    public List<string> ParsedLocationsWithDefaultBehavior { get; set; } = new();

    [JsonIgnore] 
    public int? RowInCropSpriteSheet { get; set; }
}