using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using AnimalsNeedWater.Core.Content.Models;
using AnimalsNeedWater.Core.Models;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace AnimalsNeedWater.Core.Content;

public class PlacementRegistry
{
    private readonly Dictionary<string, TroughPlacement> _caseAgnosticCache = new Dictionary<string, TroughPlacement>(StringComparer.OrdinalIgnoreCase);
    
    // diagnostics
    private readonly Dictionary<string, string> _baseSource = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TroughPlacement> _baseContent = new Dictionary<string, TroughPlacement>(StringComparer.OrdinalIgnoreCase);
    
    public void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(AssetManager.TroughPlacementsAssetName))
        {
            e.LoadFrom(BuildBaseContent, AssetLoadPriority.Medium);
        }
    }
    
    public void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(name => name.IsEquivalentTo(AssetManager.TroughPlacementsAssetName)))
        {
            _caseAgnosticCache.Clear();
            
            foreach (var building in ModEntry.BuildingTracker.AnimalBuildings)
            {
                ModEntry.TroughVisuals.ReapplyVisuals(building);
                ModEntry.PlacementDiagnostics.ValidateAndReport(building);
            }

            LogResolvedPlacements();
        }
    }
    
    public void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        LogResolvedPlacements();
    }

    
    public bool TryGetPlacement(string buildingType, [NotNullWhen(true)] out TroughPlacement? placement)
    {
        if (_caseAgnosticCache.Count == 0)
        {
            var placements = ModEntry.ModHelper.GameContent.Load<Dictionary<string, TroughPlacement>>(AssetManager.TroughPlacementsAssetName);
            foreach (var entry in placements)
            {
                _caseAgnosticCache[entry.Key] = entry.Value;
            }
        }

        return _caseAgnosticCache.TryGetValue(buildingType, out placement);
    }
    
    /// <summary>
    /// default (vanilla) + bundled profiles
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, TroughPlacement> BuildBaseContent()
    {
        var result = new Dictionary<string, TroughPlacement>(StringComparer.OrdinalIgnoreCase);
        _baseContent.Clear();
        _baseSource.Clear();
        
        // load default first
        var defaultProfile = ModEntry.ModHelper.Data.ReadJsonFile<BundledPlacementProfile>("assets/profiles/default.json");
        if (defaultProfile?.Entries == null)
        {
            ModEntry.ModMonitor.Log(
                "Unable to read default profile. Please reinstall Animals Need Water, making sure to extract all files from the archive.",
                LogLevel.Error);
            return result;
        }
        foreach (var entry in defaultProfile.Entries)
        {
            result[entry.Key] = entry.Value;
            
            // diagnostics
            _baseContent[entry.Key] = entry.Value;
            _baseSource[entry.Key] = "default";
        }
        
        // profiles override default
        var availableFiles = Directory
            .GetFiles(Path.Combine(ModEntry.ModHelper.DirectoryPath, "assets/profiles"), "*.json")
            .Where(filepath => !Path.GetFileName(filepath).Equals("default.json", StringComparison.OrdinalIgnoreCase))
            .OrderBy(filepath => filepath, StringComparer.OrdinalIgnoreCase); // A-Za-z
        foreach (var filename in availableFiles)
        {
            var bundledProfile = ModEntry.ModHelper.Data.ReadJsonFile<BundledPlacementProfile>("assets/profiles/" + Path.GetFileName(filename));
            if (bundledProfile == null || 
                bundledProfile.RequiredModId != null &&
                !ModEntry.ModHelper.ModRegistry.IsLoaded(bundledProfile.RequiredModId))
            {
                continue;
            }

            if (bundledProfile.Entries == null)
            {
                ModEntry.ModMonitor.Log(
                    $"Bundled profile for mod {bundledProfile.RequiredModId} is malformed; skipping. Please reinstall Animals Need Water, making sure to extract all files from the archive.",
                    LogLevel.Warn);
                continue;
            }

            // extend by this profile's entries, merging case-agnostically; last file sorted a-z wins
            foreach (var entry in bundledProfile.Entries)
            {
                result[entry.Key] = entry.Value;
                
                // diagnostics
                _baseContent[entry.Key] = entry.Value;
                _baseSource[entry.Key] = bundledProfile.RequiredModId ?? "unknown";
            }
        }

        return result;
    }
    
    private void LogResolvedPlacements()
    {
        // pull the live state
        var resolved = ModEntry.ModHelper.GameContent.Load<Dictionary<string, TroughPlacement>>(AssetManager.TroughPlacementsAssetName);

        ModEntry.ModMonitor.Log("Resolved placements:", LogLevel.Debug);
        foreach (var entry in resolved)
        {
            string resolvedMessage;
            if (_baseSource.TryGetValue(entry.Key, out var baseSourceForKey))
            {
                if (!_baseContent.TryGetValue(entry.Key, out var baseContentForKey))
                    continue;

                var matches = JsonConvert.SerializeObject(entry.Value).Equals(JsonConvert.SerializeObject(baseContentForKey));
                resolvedMessage = matches ? baseSourceForKey : $"content pack (overrides {baseSourceForKey})";
            }
            else
            {
                resolvedMessage = "content pack";
            }
            
            ModEntry.ModMonitor.Log($"  {entry.Key}: {resolvedMessage}", LogLevel.Debug);
        }
    }
}