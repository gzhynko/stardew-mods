using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CropGrowthAdjustments.Types
{
    public class CropAdjustment
    {
        public string CropProduceName { get; set; }
        public int CropProduceItemId { get; set; } = -1;
        public string SeasonsToGrowIn { get; set; }
        public string SeasonsToProduceIn { get; set; }
        public string LocationsWithDefaultSeasonBehavior { get; set; }
        public List<SpecialSprites> SpecialSpritesForSeasons { get; set; } = null;
        
        [JsonIgnore]
        public int RowInCropSpriteSheet { get; set; }
        
        public List<string> GetSeasonsToGrowIn()
        {
            return SeasonsToGrowIn.Split(',').ToList().Select(e => e.Trim()).ToList();
        }
        
        public List<string> GetSeasonsToProduceIn()
        {
            return SeasonsToProduceIn.Split(',').ToList().Select(e => e.Trim()).ToList();
        }
        
        public List<string> GetLocationsWithDefaultSeasonBehavior()
        {
            if (LocationsWithDefaultSeasonBehavior != null)
                return LocationsWithDefaultSeasonBehavior.Split(',').ToList().Select(e => e.Trim()).ToList();

            return new List<string>();
        }
    }
}