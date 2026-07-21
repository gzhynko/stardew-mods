using System.Collections.Generic;
using AnimalsNeedWater.Core.Models;

namespace AnimalsNeedWater.Core.Multiplayer.Models;

public class ThirstyAnimalsMessage
{
    public readonly List<ThirstyAnimalInfo> ThirstyAnimals;

    public ThirstyAnimalsMessage(List<ThirstyAnimalInfo> thirstyAnimals)
    {
        ThirstyAnimals = thirstyAnimals;
    }

}