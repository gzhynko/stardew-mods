using System.Collections.Generic;
using Newtonsoft.Json;
using StardewModdingAPI;

namespace CropGrowthAdjustments.Types
{
    public class Adjustments
    {
        public List<CropAdjustment> CropAdjustments { get; set; }
        
        [JsonIgnore] 
        public IContentPack ContentPack { get; set; }
    }
}