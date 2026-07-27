using System;
using System.Collections.Generic;
using System.Linq;
using AnimalsNeedWater.Core.Models;
using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewValley.Extensions;
using Object = StardewValley.Object;

namespace AnimalsNeedWater.Core.Game;

public class BuildingWaterManager
{
    public void MarkWatered(Building building)
    {
        ModEntry.Data.BuildingsWithWateredTrough.Add(building.GetIndoorsName());
    }

    public void MarkWateredName(string name)
    {
        ModEntry.Data.BuildingsWithWateredTrough.Add(name);
    }

    public void ClearWatered(Building building)
    {
        ModEntry.Data.BuildingsWithWateredTrough.Remove(building.GetIndoorsName());
    }

    public void MarkBonusAwardedName(string name)
    {
        ModEntry.Data.BuildingsWithBonusAwardedToday.Add(name);
    }

    public void ClearBonus(Building building)
    {
        ModEntry.Data.BuildingsWithBonusAwardedToday.Remove(building.GetIndoorsName());
    }

    public bool BuildingHasTroughsWatered(Building building)
    {
        return ModEntry.Data.BuildingsWithWateredTrough.Contains(building.GetIndoorsName());
    }

    public bool BuildingHasFullWaterSource(Building building)
    {
        var indoors = building.GetIndoors();
        if (indoors == null)
            return false;

        foreach (Object obj in indoors.Objects.Values)
        {
            if (ModEntry.WaterSources.TryGet(obj, out var src) && ModEntry.WaterSources.IsFull(obj, src))
            {
                return true;
            }
        }

        return false;
    }


    public bool IsWatered(Building building)
    {
        return BuildingHasTroughsWatered(building) || BuildingHasFullWaterSource(building);
    }

    public void WaterBuilding(Building building)
    {
        // fill sources before the profile check
        var indoors = building.GetIndoors();
        if (indoors != null)
        {
            foreach (var obj in indoors.Objects.Values)
            {
                if (ModEntry.WaterSources.TryGet(obj, out var src))
                {
                    ModEntry.WaterSources.Fill(obj, src);
                }
            }
        }

        var buildingName = building.buildingType.Value;
        if (!ModEntry.PlacementRegistry.TryGetPlacement(buildingName, out _))
            return;

        MarkWatered(building);
    }

    public void FillSprinklerCoveredSources(Building building)
    {
        if (!ModEntry.Config.SprinklersFillWaterBowls) return;

        var indoors = building.GetIndoors();
        if (indoors == null) return;

        // gather every tile any sprinkler in here covers
        var covered = new HashSet<Vector2>();
        foreach (var obj in indoors.Objects.Values)
        {
            if (!obj.IsSprinkler()) continue;
            foreach (var t in obj.GetSprinklerTiles())
            {
                covered.Add(t);
            }
        }

        if (covered.Count == 0) return;

        foreach (var obj in indoors.Objects.Values)
        {
            if (covered.Contains(obj.TileLocation)
                && ModEntry.WaterSources.TryGet(obj, out var src)
                && src.FillableBySprinkler)
            {
                ModEntry.WaterSources.Fill(obj, src);
            }
        }
    }

    public void EmptyWaterSources(Building building)
    {
        var indoors = building.GetIndoors();
        if (indoors == null) return;
        foreach (Object obj in indoors.Objects.Values)
        {
            if (ModEntry.WaterSources.TryGet(obj, out var src))
            {
                ModEntry.WaterSources.Empty(obj, src);
            }
        }
    }

    public void EmptyBuilding(Building building)
    {
        var buildingName = building.GetIndoorsName();
        var buildingType = building.buildingType.Value;

        var residents = building.GetParentLocation().getAllFarmAnimals()
            .Where(a => a.home != null && a.home.GetIndoorsName()
                .Equals(buildingName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // no residents; noop
        if (residents.Count == 0)
        {
            return;
        }

        var allDrankOutside = ModEntry.Config.TroughsCanRemainFull
                              && residents.All(a => ModEntry.Data.IsAnimalFull(a));

        // water sources empty daily unless everyone drank outside
        if (!allDrankOutside)
        {
            EmptyWaterSources(building);
        }

        // everyone drank outside -> keep old state
        if (allDrankOutside && ModEntry.Data.BuildingsWithWateredTrough.Contains(buildingName))
        {
            return;
        }

        if (ModEntry.Config.UseWateringSystems)
        {
            if (ModEntry.PlacementRegistry.TryGetPlacement(buildingType, out var buildingProfile) &&
                buildingProfile.WateringSystemTiles.Count != 0)
            {
                // water trough -> mark watered
                MarkWatered(building);
                return;
            }
        }

        ClearWatered(building);
    }
}