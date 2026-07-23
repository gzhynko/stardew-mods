namespace FishExclusions.Core.Models;

/// <summary> The mod config class. More info here: https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Config </summary>
public class ModConfig
{
    /// <summary>
    /// The items to exclude.
    /// </summary>
    public ItemsToExclude ItemsToExclude { get; set; } = new();

    /// <summary>
    /// The ID of the item to catch if all possible fish for this water body / season / weather is excluded.
    /// </summary>
    public string ItemToCatchIfAllFishIsExcluded { get; set; } = "168";

    /// <summary>
    /// The number of times to retry fish selection before giving up and catching the item specified above.
    /// </summary>
    public int TimesToRetry { get; set; } = 20;
}