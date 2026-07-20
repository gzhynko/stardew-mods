using System.Collections.Generic;
using System.Linq;
using AnimalsNeedWater.Core.Models;
using StardewValley;
// ReSharper disable InconsistentNaming

namespace AnimalsNeedWater.Core;

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
        return ModEntry.ThirstTracker.AnimalsLeftThirstyYesterday.ConvertAll(i => i.Id);
    }

    public bool WasAnimalLeftThirstyYesterday(FarmAnimal animal)
    {
        return ModEntry.ThirstTracker.WasAnimalLeftThirstyYesterday(animal);
    }

    public List<string> GetBuildingsWithWateredTrough()
    {
        return ModEntry.Data.BuildingsWithWateredTrough.ToList();
    }

    public bool IsAnimalFull(FarmAnimal animal)
    {
        return ModEntry.Data.IsAnimalFull(animal);
    }
    
    public bool DoesAnimalHaveAccessToWater(FarmAnimal animal)
    {
        if (animal.home == null) return ModEntry.Data.IsAnimalFull(animal);
        
        return ModEntry.TroughManager.IsWatered(animal.home) || ModEntry.Data.IsAnimalFull(animal);
    }

    public List<long> GetFullAnimals()
    {
        return ModEntry.Data.FullAnimals.ToList();
    }
}
