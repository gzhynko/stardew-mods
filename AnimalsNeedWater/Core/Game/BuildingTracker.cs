using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;

namespace AnimalsNeedWater.Core.Game;

public class BuildingTracker
{
    public List<Building> AnimalBuildings = new List<Building>();
    // group buildings by their parent location
    public IEnumerable<IGrouping<GameLocation, Building>> AnimalBuildingGroups = [];

    public void Refresh()
    {
        AnimalBuildings.Clear();
        foreach (GameLocation location in Game1.locations)
        {
            foreach (Building building in location.buildings)
            {
                if (building.GetIndoors() is AnimalHouse)
                {
                    AnimalBuildings.Add(building);
                }
            }
        }
        AnimalBuildingGroups = AnimalBuildings.GroupBy(b => b.GetParentLocation()).ToArray(); // materialize
    }
    
    public void CheckHomeStatus()
    {
        bool needToFixAnimals = false;

        foreach (var locationGroup in AnimalBuildingGroups)
        {
            GameLocation parentLocation = locationGroup.Key;

            if (parentLocation.getAllFarmAnimals().Any(animal => animal.home == null))
            {
                needToFixAnimals = true;
                break;
            }
        }

        if (needToFixAnimals)
        {
            StardewValley.Utility.fixAllAnimals();
        }
    }
}