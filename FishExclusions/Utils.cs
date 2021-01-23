using System.Collections.Generic;
using System.Linq;

namespace FishExclusions
{
    internal class Utils
    {
        public static int[] GetExcludedFish(ModConfig config, string seasonName, string locationName)
        {
            // Initialize the list with common exclusions (since they are always excluded).
            List<int> excludedFish = config.ItemsToExclude.CommonExclusions.ToList();

            foreach (ConditionalExclusion exclusion in config.ItemsToExclude.ConditionalExclusions)
            {
                string exclusionSeason = exclusion.Season.ToLower();
                string exclusionLocation = exclusion.Location.ToLower();
            }

            return excludedFish.ToArray<int>();
        }
    }
}