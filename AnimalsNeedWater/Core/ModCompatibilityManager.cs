using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AnimalsNeedWater.Core.Content;
using AnimalsNeedWater.Core.Content.Models;
using AnimalsNeedWater.Core.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Buildings;

namespace AnimalsNeedWater.Core;

public class ModCompatibilityManager
{
    private static BuildingTroughPlacement? GetTroughPlacementForBuilding(string buildingName)
    {
        return GetTroughPlacementForBuildingWithTargetModId(buildingName)?.Item1;
    }

    private static List<string> GetCoveredBuildings()
    {
        HashSet<string> coverage = new ();
        
        // profiles
        foreach (var profileBuilding in LegacyProfileManager.ActiveProfiles.Keys)
        {
            coverage.Add(profileBuilding);
        }
        // mod compat placements
        foreach (var config in ContentManager.GetTroughPlacements())
        {
            foreach (var placementBuilding in config.Placement.Keys)
            {
                coverage.Add(placementBuilding);
            }
        }

        return coverage.ToList();
    }

    private static void LoadLegacyProfiles()
    {
        try
        {
            LegacyProfileManager.LoadProfiles();
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Error while loading legacy trough placement profiles: {e}", LogLevel.Warn);
        }
        
        LegacyProfileManager.FindActiveProfiles();
    }

    private static void Initialize()
    {
        LoadLegacyProfiles();
        
        // look for conflicts
        var troughPlacements = ContentManager.GetTroughPlacements();
        var buildingCoverage = new Dictionary<string, List<string>>();
        foreach (var configuration in troughPlacements)
        {
            if (configuration.ModId == ModEntry.ModHelper.ModRegistry.ModID) continue;
            
            foreach (var building in configuration.Placement.Keys)
            {
                if (buildingCoverage.ContainsKey(building))
                    buildingCoverage[building].Add(configuration.ModId);
                else
                    buildingCoverage[building] = new List<string> { configuration.ModId };
            }
        }

        foreach (var (building, modIds) in buildingCoverage)
        {
            if (modIds.Count <= 1) continue;
            ModEntry.ModMonitor.Log(
                $"Building {building} has multiple conflicting trough configurations provided by the following mods: " +
                string.Join(", ", modIds) + ". This might result in unexpected behavior.", LogLevel.Warn);
        }
        
        ModEntry.ModMonitor.Log("Using the following trough placement configurations:", LogLevel.Info);
        foreach (var buildingName in GetCoveredBuildings())
        {
            var placement = GetTroughPlacementForBuildingWithTargetModId(buildingName)!;
            var modId = placement.Item2;
            
            ModEntry.ModMonitor.Log($"  {buildingName}: {(modId == ModEntry.ModHelper.ModRegistry.ModID ? "default" : modId)}", LogLevel.Info);
        }
    }

    public static void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        Initialize();
    }
    
    private static Tuple<BuildingTroughPlacement, string>? GetTroughPlacementForBuildingWithTargetModId(string buildingName)
    {
        Tuple<BuildingTroughPlacement, string>? result = null;
        // prefer content pipeline trough placement configs over the legacy profiles
        foreach (var config in ContentManager.GetTroughPlacements())
        {
            foreach (var placementBuilding in config.Placement.Keys)
            {
                if (buildingName == placementBuilding)
                    result = new Tuple<BuildingTroughPlacement, string>(config.Placement[placementBuilding], config.ModId);
            }
        }

        if (result != null)
            return result;

        if (LegacyProfileManager.ActiveProfiles.ContainsKey(buildingName))
        {
            var placement = LegacyProfileManager.ActiveProfiles[buildingName][0].GetPlacementForBuildingName(buildingName);
            var modId = LegacyProfileManager.ActiveProfiles[buildingName][0].ModUniqueId;
            result = new Tuple<BuildingTroughPlacement, string>(placement, modId);
        }
        
        return result;
    }

}