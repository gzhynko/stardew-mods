using System.Collections.Generic;

namespace AnimalsNeedWater.Core.Models;

public class StateSyncMessage
{
    public readonly HashSet<string> BuildingsWithWateredTrough;
    public readonly HashSet<long> FullAnimals;

    public StateSyncMessage(HashSet<string> buildingsWithWateredTrough,  HashSet<long> fullAnimals)
    {
        BuildingsWithWateredTrough = buildingsWithWateredTrough;
        FullAnimals = fullAnimals;
    }

}