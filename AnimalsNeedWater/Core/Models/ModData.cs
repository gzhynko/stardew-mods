using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StardewValley;

namespace AnimalsNeedWater.Core.Models;

/// <summary> Contains global variables and constants for the mod. </summary>
public class ModData
{
    public HashSet<string> BuildingsWithWateredTrough { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public HashSet<long> FullAnimals { get; set; } = new HashSet<long>();
        
    public bool IsAnimalFull(FarmAnimal animal) => FullAnimals.Contains(animal.myID.Value);
    public void ResetFullAnimals() => FullAnimals = new HashSet<long>();
}