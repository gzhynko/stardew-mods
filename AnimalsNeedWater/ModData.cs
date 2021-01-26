using System.Collections.Generic;
using StardewValley;

namespace AnimalsNeedWater
{
    /// <summary> Contains global variables for the mod. </summary>
    public static class ModData
    {
        public static List<string> CoopsWithWateredTrough { get; set; } = new List<string>();
        public static List<string> BarnsWithWateredTrough { get; set; } = new List<string>();
        public static List<FarmAnimal> FullAnimals { get; set; } = new List<FarmAnimal>();
    }
}
