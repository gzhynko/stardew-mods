using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StardewValley;

namespace AnimalsNeedWater.Core.Models;

/// <summary> Contains global variables and constants for the mod. </summary>
public class ModData
{
    public HashSet<string> BuildingsWithWateredTrough { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> BuildingsWithBonusAwardedToday { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public HashSet<long> FullAnimals { get; set; } = new HashSet<long>();
        
    public bool IsAnimalFull(FarmAnimal animal) => FullAnimals.Contains(animal.myID.Value);
    public void ResetFullAnimals() => FullAnimals = new HashSet<long>();
    
    public void RestoreComparers()
    {
        BuildingsWithWateredTrough = new HashSet<string>(
            BuildingsWithWateredTrough ?? Enumerable.Empty<string>(),
            StringComparer.OrdinalIgnoreCase
        );

        BuildingsWithBonusAwardedToday = new HashSet<string>(
            BuildingsWithBonusAwardedToday ?? Enumerable.Empty<string>(),
            StringComparer.OrdinalIgnoreCase
        );
    }
}