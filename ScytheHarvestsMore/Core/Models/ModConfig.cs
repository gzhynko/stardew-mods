namespace ScytheHarvestsMore.Core.Models;

/// <summary> The mod config class. More info here: https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Config </summary>
public class ModConfig
{
    /// <summary>
    /// Whether the iridium scythe harvests machines.
    /// </summary>
    public bool HarvestMachines { get; set; } = true;

    /// <summary>
    /// Whether the iridium scythe harvests fruit trees.
    /// </summary>
    public bool HarvestFruitTrees { get; set; } = true;

    /// <summary>
    /// Whether the iridium scythe harvests forage items.
    /// </summary>
    public bool HarvestForage { get; set; } = true;

    /// <summary>
    /// Whether the iridium scythe digs up ginger crops.
    /// </summary>
    public bool HarvestGinger { get; set; } = true;

    /// <summary>
    /// Whether the iridium scythe harvests berry bushes.
    /// </summary>
    public bool HarvestBerryBushes { get; set; } = true;
    
    /// <summary>
    /// Whether the iridium scythe shakes grown wild trees.
    /// </summary>
    public bool ShakeTrees { get; set; } = true;
    
    /// <summary>
    /// Whether the iridium scythe harvests crab pots.
    /// </summary>
    public bool HarvestCrabPots { get; set; } = true;
}