using System.Collections.Generic;

namespace AnimalsNeedWater.Core.Models;

public class ThirstyAnimalsMessage
{
    public readonly List<ThirstyAnimalInfo> ThirstyAnimals;

    public ThirstyAnimalsMessage(List<ThirstyAnimalInfo> thirstyAnimals)
    {
        ThirstyAnimals = thirstyAnimals;
    }

}