using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CropGrowthAdjustments.Types
{
    public class SpecialSpriteForSeason
    {
        public string Season { get; set; }
        public string Sprites { get; set; }
        public string LocationsToIgnore { get; set; }
        
        [JsonIgnore]
        public int RowInSpriteSheet { get; set; }

        public List<string> GetLocationsToIgnore()
        {
            if (LocationsToIgnore != null)
                return LocationsToIgnore.Split(',').ToList().Select(e => e.Trim()).ToList();

            return new List<string>();
        }
    }
}