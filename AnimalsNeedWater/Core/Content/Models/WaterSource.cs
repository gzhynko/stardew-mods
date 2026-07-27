namespace AnimalsNeedWater.Core.Content.Models;

/// <summary>
/// parsed from Data/BigCraftables customfields
/// </summary>
public class WaterSource
{
    public bool NeedsRefilling = true;
    public bool FillableByCan = true;
    public bool FillableBySprinkler = true;
    public int EmptySpriteIndex = 0;
    public int FullSpriteIndex = 1;
}