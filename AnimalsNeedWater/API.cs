using System.Collections.Generic;
using StardewValley;

namespace AnimalsNeedWater
{
    public interface IAnimalsNeedWaterAPI
    {
        List<FarmAnimal> GetAnimalsLeftThirstyYesterday();

        List<string> GetCoopsWithWateredTrough();
        List<string> GetBarnsWithWateredTrough();

        bool IsAnimalFull(FarmAnimal animal);
        List<FarmAnimal> GetFullAnimals();
    }

    public class API : IAnimalsNeedWaterAPI
    {
        public List<FarmAnimal> GetAnimalsLeftThirstyYesterday()
        {
            return ModEntry.Instance.AnimalsLeftThirstyYesterday;
        }

        public List<string> GetCoopsWithWateredTrough()
        {
            return ModData.CoopsWithWateredTrough;
        }

        public List<string> GetBarnsWithWateredTrough()
        {
            return ModData.BarnsWithWateredTrough;
        }

        public bool IsAnimalFull(FarmAnimal animal)
        {
            return ModData.FullAnimals.Contains(animal);
        }

        public List<FarmAnimal> GetFullAnimals()
        {
            return ModData.FullAnimals;
        }
    }
}
