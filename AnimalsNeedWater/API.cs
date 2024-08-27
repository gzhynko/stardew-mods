using System.Collections.Generic;
using AnimalsNeedWater.Types;
using StardewValley;
// ReSharper disable InconsistentNaming

namespace AnimalsNeedWater
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
            return ModData.BuildingsWithWateredTrough;
        }

        public bool IsAnimalFull(FarmAnimal animal)
        {
            return ModData.FullAnimals.Contains(animal);
        }
        
        public bool DoesAnimalHaveAccessToWater(FarmAnimal animal)
        {
            var houseTroughFull = ModData.BuildingsWithWateredTrough.Contains(animal.home.GetIndoorsName().ToLower());
            return houseTroughFull || ModData.FullAnimals.Contains(animal);
        }

        public List<long> GetFullAnimals()
        {
            return ModData.FullAnimals.ConvertAll(i => i.myID.Value);
        }
    }
}
