using AnimalsNeedWater.Models;
using StardewValley;

namespace AnimalsNeedWater;

public class Utility
{
    public static void EmptyWaterBowlObject(Object waterBowl)
    {
        waterBowl.modData[ModConstants.WaterBowlItemModDataIsFullField] = "false";
        waterBowl.ParentSheetIndex = 0;
    }
    
    public static void FillWaterBowlObject(Object waterBowl)
    {
        waterBowl.modData[ModConstants.WaterBowlItemModDataIsFullField] = "true";
        waterBowl.ParentSheetIndex = 1;
    }
}