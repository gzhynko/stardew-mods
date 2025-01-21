using System.Collections.Generic;
using Newtonsoft.Json;
using StardewValley;

namespace AnimalsNeedWater.Core.Models
{
    /// <summary> Contains global variables and constants for the mod. </summary>
    public class ModData
    {
        public List<string> BuildingsWithWateredTrough { get; set; } = new List<string>();
        public List<long> FullAnimalsInternal { get; set; } = new List<long>();
        
        [JsonIgnore] 
        public List<FarmAnimal> FullAnimals = null!;

        public void ParseInternalFields()
        {
            FullAnimals = FullAnimalsInternal.ConvertAll(StardewValley.Utility.getAnimal);
        }
        public void AddFullAnimal(FarmAnimal animal) => FullAnimalsInternal.Add(animal.myID.Value);
        public bool IsAnimalFull(FarmAnimal animal) => FullAnimalsInternal.Contains(animal.myID.Value);
        public void ResetFullAnimals() => FullAnimalsInternal = new List<long>();
    }
}