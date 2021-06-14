using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CropGrowthAdjustments.Types
{
    public class CropAdjustment
    {
        public string CropProduceName { get; set; }
        public string SeasonsToGrowIn { get; set; }
        public string SeasonsToProduceIn { get; set; }
        public string LocationsToGrowAllYearRoundIn { get; set; }
        public List<SpecialSpriteForSeason> SpecialSpritesForSeasons { get; set; } = null;
        
        [JsonIgnore]
        public int CropProduceItemId { get; set; }
        
        [JsonIgnore]
        public int OriginalRowInSpriteSheet { get; set; }
        
        public List<string> GetSeasonsToGrowIn()
        {
            return SeasonsToGrowIn.Split(',').ToList().Select(e => e.Trim()).ToList();
        }
        
        public List<string> GetSeasonsToProduceIn()
        {
            return SeasonsToProduceIn.Split(',').ToList().Select(e => e.Trim()).ToList();
        }
        
        public List<string> GetLocationsToGrowAllYearRoundIn()
        {
            if (LocationsToGrowAllYearRoundIn != null)
                return LocationsToGrowAllYearRoundIn.Split(',').ToList().Select(e => e.Trim()).ToList();

            return new List<string>();
        }
    }
}