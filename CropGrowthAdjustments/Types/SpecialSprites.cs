using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace CropGrowthAdjustments.Types
{
    public class SpecialSprites
    {
        public string Season { get; set; }
        public string Sprites { get; set; }
        public string LocationsToIgnore { get; set; }
        
        [JsonIgnore]
        public Texture2D SpritesTexture { get; set; }

        public List<string> GetLocationsToIgnore()
        {
            if (LocationsToIgnore != null)
                return LocationsToIgnore.Split(',').ToList().Select(e => e.Trim()).ToList();

            return new List<string>();
        }
    }
}