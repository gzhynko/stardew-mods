namespace AnimalsNeedWater.Core.Multiplayer.Models;

public class AddFullAnimalMessage
{
    public readonly long AnimalId;

    public AddFullAnimalMessage(long animalId)
    {
        AnimalId = animalId;
    }
}