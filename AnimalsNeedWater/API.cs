using System.Collections.Generic;

namespace AnimalsNeedWater
{
    public interface IAnimalsNeedWaterAPI
    {
        List<ModEntry.AnimalLeftThirsty> GetAnimalsLeftThirstyYesterday();

        List<string> GetCoopsWithWateredTrough();
        List<string> GetBarnsWithWateredTrough();
    }

    public class API : IAnimalsNeedWaterAPI
    {
        public List<ModEntry.AnimalLeftThirsty> GetAnimalsLeftThirstyYesterday()
        {
            return ModEntry.instance.AnimalsLeftThirstyYesterday;
        }

        public List<string> GetCoopsWithWateredTrough()
        {
            return ModData.CoopsWithWateredTrough;
        }

        public List<string> GetBarnsWithWateredTrough()
        {
            return ModData.BarnsWithWateredTrough;
        }
    }
}
