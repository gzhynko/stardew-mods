using System.Collections.Generic;
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
    }
}
