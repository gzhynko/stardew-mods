using System;
using System.Collections.Generic;
using System.Linq;
using AnimalsNeedWater.Core.Content.Models;
using AnimalsNeedWater.Core.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace AnimalsNeedWater.Core.Content;

public static class ContentManager
{
    private static string TroughPlacementsAssetName = $"Mods/{ModEntry.Manifest.UniqueID}/TroughPlacements";
    private static string TroughTilesheetsAssetName = $"Mods/{ModEntry.Manifest.UniqueID}/TroughTilesheets";
    private static string WateringSystemTilesheetsAssetName = $"Mods/{ModEntry.Manifest.UniqueID}/WateringSystemTilesheets";
    private static string EmptyBuildingSpritesAssetName = $"Mods/{ModEntry.Manifest.UniqueID}/EmptyBuildingSprites";

    private static T? LoadAssetDependency<T>(string modId, string assetPath) where T : class
    {
        try
        {
            return ModEntry.ModHelper.GameContent.Load<T>(assetPath);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Unable to load mod compatibility asset dependency at path {assetPath} (type: {nameof(T)}) introduced by mod {modId}: {e}", LogLevel.Error);
            return null;
        }
    }
    
    public static IEnumerable<TroughPlacementConfiguration> GetTroughPlacements()
    {
        var troughPlacementsAsset = ModEntry.ModHelper.GameContent.Load<Dictionary<string, TroughPlacementModel>>(TroughPlacementsAssetName);

        foreach (var (modId, troughPlacementModel) in troughPlacementsAsset)
        {
            var placement = LoadAssetDependency<TroughPlacementConfiguration>(modId, troughPlacementModel.File);
            if (placement == null) continue;
            placement.ModId = modId;
            yield return Utils.TroughPlacementRetainSelectBuildings(placement, troughPlacementModel.Buildings);
        }
    }
    
    public static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        
    }
}