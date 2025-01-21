using AnimalsNeedWater.Core.Models;
using StardewValley;

namespace AnimalsNeedWater.Core;

public static class Utils
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