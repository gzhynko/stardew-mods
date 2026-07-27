using System.Collections.Generic;

namespace AnimalsNeedWater.Core.Multiplayer.Models;

public class StateSyncMessage
{
    public readonly HashSet<string> BuildingsWithWateredTrough;
    public readonly HashSet<string> BuildingsWithBonusAwardedToday;
    public readonly HashSet<long> FullAnimals;

    public StateSyncMessage(HashSet<string> buildingsWithWateredTrough, HashSet<string> buildingsWithBonusAwardedToday,  HashSet<long> fullAnimals)
    {
        BuildingsWithWateredTrough = buildingsWithWateredTrough;
        BuildingsWithBonusAwardedToday = buildingsWithBonusAwardedToday;
        FullAnimals = fullAnimals;
    }

}