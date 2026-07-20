using System;
using System.Collections.Generic;
using System.Linq;
using AnimalsNeedWater.Core.Models;
using StardewValley.Buildings;
using StardewValley.Extensions;
using Object = StardewValley.Object;

namespace AnimalsNeedWater.Core.Game;

public class TroughManager
{
    public void MarkWatered(Building building)
    {
        ModEntry.Data.BuildingsWithWateredTrough.Add(building.GetIndoorsName());
    }

    public void ClearWatered(Building building)
    {
        ModEntry.Data.BuildingsWithWateredTrough.Remove(building.GetIndoorsName());
    }
    
    public bool BuildingHasTroughsWatered(Building building)
    {
        return ModEntry.Data.BuildingsWithWateredTrough.Contains(building.GetIndoorsName());
    }
    
    public bool BuildingHasFullWaterBowl(Building building)
    {
        var hasFullWaterBowlObject = false;
        var buildingObjects = building.GetIndoors().Objects.Values;
        foreach (Object @object in buildingObjects)
        {
            if (!@object.HasTypeId("(BC)") || @object.ItemId != ModConstants.WaterBowlItemId) 
                continue;
            if (@object.modData.ContainsKey(ModConstants.WaterBowlItemModDataIsFullField)
                && @object.modData[ModConstants.WaterBowlItemModDataIsFullField] == "true")
            {
                hasFullWaterBowlObject = true;
            }
        }

        return hasFullWaterBowlObject;
    }

    public bool IsWatered(Building building)
    {
        return BuildingHasTroughsWatered(building) ||  BuildingHasFullWaterBowl(building);
    }
    
    public void FillAllWaterTroughs()
    {
        foreach (Building building in ModEntry.BuildingTracker.AnimalBuildings)
        {
            var buildingName = building.buildingType.Value;
            if (!ModEntry.PlacementRegistry.TryGetPlacement(buildingName, out _))
                continue;
                
            MarkWatered(building);
        }
    }


    public void EmptyWaterBowls(Building building)
    {
        var buildingObjects = building.GetIndoors().Objects.Values;
        foreach (Object obj in buildingObjects)
        {
            if (!obj.HasTypeId("(BC)") || obj.ItemId != ModConstants.WaterBowlItemId) 
                continue;
            if (obj.modData.ContainsKey(ModConstants.WaterBowlItemModDataIsFullField)
                && obj.modData[ModConstants.WaterBowlItemModDataIsFullField] == "true")
            {
                Utils.EmptyWaterBowlObject(obj);
            }
        }
    }
    
    public void EmptyWaterTroughs()
    {
        var previouslyWatered = new HashSet<string>(
            ModEntry.Data.BuildingsWithWateredTrough, StringComparer.OrdinalIgnoreCase);
        ModEntry.Data.BuildingsWithWateredTrough.Clear();
        
        foreach (Building building in ModEntry.BuildingTracker.AnimalBuildings)
        {
            var indoors = building.indoors.Value;
            var allDrankOutside = ModEntry.Config.TroughsCanRemainFull && indoors.animals.Values.All(a => ModEntry.Data.IsAnimalFull(a));

            if (!allDrankOutside)
            {
                EmptyWaterBowls(building);
            }
            
            // preserve trough only if it was actually full yesterday
            if (allDrankOutside && previouslyWatered.Contains(building.GetIndoorsName()))
            {
                MarkWatered(building);
                continue;
            }
            
            if (ModEntry.Config.UseWateringSystems)
            {
                // check if this building has a watering system
                var buildingName = building.buildingType.Value;
                if (ModEntry.PlacementRegistry.TryGetPlacement(buildingName, out var buildingProfile) && buildingProfile.WateringSystemTiles.Count != 0)
                {
                    MarkWatered(building);
                    
                    // do not proceed with emptying water troughs
                    continue;
                } 
            }

            // if no animals live here, do not empty the water trough
            int animalCount = building.GetParentLocation().getAllFarmAnimals().Count(animal => animal.home != null && animal.home.GetIndoorsName().Equals(building.GetIndoorsName(), StringComparison.OrdinalIgnoreCase));
            if (animalCount == 0)
            {
                MarkWatered(building);
                continue;
            }
        }
    }
}