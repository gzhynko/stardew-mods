using System.Collections.Generic;

namespace AnimalsNeedWater.Core.Models;

public class AddFullAnimalMessage
{
    public readonly long AnimalId;

    public AddFullAnimalMessage(long animalId)
    {
        AnimalId = animalId;
    }

}