using System.Collections.Generic;
using StardewValley;

namespace AnimalsNeedWater.Types
{
    /// <summary> Contains global variables and constants for the mod. </summary>
    public static class ModData
    {
        public static List<string> BuildingsWithWateredTrough { get; set; } = new List<string>();
        public static List<FarmAnimal> FullAnimals { get; set; } = new List<FarmAnimal>();
        public static int LoveEmote { get; } = 20;
        
        public static readonly string WaterBowlItemId = $"{ModEntry.ModHelper.ModContent.ModID}_Water_Bowl";
        public static readonly string WaterBowlItemModDataIsFullField = $"{ModEntry.ModHelper.ModContent.ModID}_Water_Bowl.IsFull";
    }
}