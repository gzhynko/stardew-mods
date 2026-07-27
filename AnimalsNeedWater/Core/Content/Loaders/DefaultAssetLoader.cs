using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

namespace AnimalsNeedWater.Core.Content.Loaders;

public class DefaultAssetLoader
{
    private static readonly Dictionary<string, string> DefaultAssets = new(StringComparer.OrdinalIgnoreCase)
    {
        [AssetManager.CoopEmptyWaterTroughOverlayAssetName] = AssetManager.CoopEmptyWaterTroughOverlay,
        [AssetManager.BigCoopEmptyWaterTroughOverlayAssetName] = AssetManager.Coop2EmptyWaterTroughOverlay,
        [AssetManager.WateringSystemTilesheetAssetName] = AssetManager.WateringSystemTilesheet,
    };

    public static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(AssetManager.WaterTroughTilesheetAssetName))
        {
            var file = ModEntry.Config.CleanerTroughs
                ? AssetManager.WaterTroughTilesheetClean
                : AssetManager.WaterTroughTilesheet;
            e.LoadFromModFile<Texture2D>(file, AssetLoadPriority.Medium);
            return;
        }

        if (DefaultAssets.TryGetValue(e.NameWithoutLocale.BaseName, out var modFilePath))
        {
            e.LoadFromModFile<Texture2D>(modFilePath, AssetLoadPriority.Medium);
        }
    }
}
