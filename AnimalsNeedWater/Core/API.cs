using System.Collections.Generic;
using System.Linq;
using AnimalsNeedWater.Core.Models;
using StardewValley;
// ReSharper disable InconsistentNaming

namespace AnimalsNeedWater.Core
{
    public interface IAnimalsNeedWaterAPI
    {
        List<long> GetAnimalsLeftThirstyYesterday();
        bool WasAnimalLeftThirstyYesterday(FarmAnimal animal);

        List<string> GetBuildingsWithWateredTrough();

        bool IsAnimalFull(FarmAnimal animal);
        bool DoesAnimalHaveAccessToWater(FarmAnimal animal);
        List<long> GetFullAnimals();
        bool IsTileWaterTrough(int tileX, int tileY);
    }

    public class API : IAnimalsNeedWaterAPI
    {
        public List<long> GetAnimalsLeftThirstyYesterday()
        {
            return ModEntry.AnimalsLeftThirstyYesterday.ConvertAll(i => i.myID.Value);
        }

        public bool WasAnimalLeftThirstyYesterday(FarmAnimal animal)
        {
            return ModEntry.AnimalsLeftThirstyYesterday.Contains(animal);
        }

        public List<string> GetBuildingsWithWateredTrough()
        {
            return ModEntry.Data.BuildingsWithWateredTrough;
        }

        public bool IsAnimalFull(FarmAnimal animal)
        {
            return ModEntry.Data.IsAnimalFull(animal);
        }
        
        public bool DoesAnimalHaveAccessToWater(FarmAnimal animal)
        {
            var houseTroughFull = ModEntry.Data.BuildingsWithWateredTrough.Contains(animal.home.GetIndoorsName().ToLower());
            return houseTroughFull || ModEntry.Data.IsAnimalFull(animal);
        }

        public List<long> GetFullAnimals()
        {
            return ModEntry.Data.FullAnimalsInternal;
        }

        public bool IsTileWaterTrough(int tileX, int tileY)
        {
            var building = Game1.currentLocation?.ParentBuilding;
            if (building == null)
                return false;

            var buildingType = building.buildingType?.Value;
            var buildingProfile = ModEntry.GetProfileForBuilding(buildingType);
            if (buildingProfile == null)
                return false;

            var placement = buildingProfile.GetPlacementForBuildingName(buildingType);
            return placement.TroughTiles.Any(t => t.TileX == tileX && t.TileY == tileY);

        }
    }
}
