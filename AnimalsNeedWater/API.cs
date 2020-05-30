using System.Collections.Generic;

namespace AnimalsNeedWater
{
    public interface IAPI
    {
        List<ModEntry.AnimalLeftThirsty> GetAnimalsLeftThirstyYesterday();

        List<string> GetCoopsWithWateredTrough();
        List<string> GetBarnsWithWateredTrough();
    }

    public class API : IAPI
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
