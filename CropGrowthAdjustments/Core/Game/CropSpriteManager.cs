using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace CropGrowthAdjustments.Core.Game;

public class CropSpriteManager
{
    private const string DefaultCropSheetName = "TileSheets\\crops";

    public void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (Context.IsMainPlayer)
        {
            StardewValley.Utility.ForEachLocation(location =>
            {
                ForEachHoeDirt(location, ApplySpritesFor);
                return true;
            });
        }

        RefreshDrawnTextures(Game1.currentLocation);
    }

    public void OnWarped(object? sender, WarpedEventArgs e)
    {
        RefreshDrawnTextures(e.NewLocation);
    }

    public static void ApplySpritesFor(HoeDirt hoeDirt)
    {
        var crop = hoeDirt.crop;
        var location = hoeDirt.Location;
        if (crop == null || location == null || crop.dead.Value) return;

        var cropHarvestId = Utility.NormalizeItemId(crop.indexOfHarvest.Value ?? "");
        var season = location.GetSeason();

        foreach (var contentPack in ModEntry.ContentPackLoader.ContentPacks)
        {
            foreach (var adjustment in contentPack.CropAdjustments)
            {
                // skip if this crop is not the desired one.
                if (cropHarvestId != Utility.NormalizeItemId(adjustment.CropProduceItemId)) continue;
                // skip if there are no usable special sprites for this crop (either none configured or its row never resolved)
                if (adjustment.SpecialSpritesForSeasons.All(s => s.GeneratedAssetName == null)) continue;

                // null means let the game derive default texture from crop data
                string? desired = null;
                foreach (var specialSprite in adjustment.SpecialSpritesForSeasons)
                {
                    if (specialSprite.GeneratedAssetName == null) continue;
                    // skip if the crop is planted in any of the ignored locations.
                    if (Utility.IsInAnyOfSpecifiedLocations(specialSprite.ParsedLocationsToIgnore, location)) continue;
                    // skip if this special sprite should not be applied in this season.
                    if (specialSprite.ParsedSeason != season) continue;

                    desired = specialSprite.GeneratedAssetName;
                }

                var desiredValue = desired ?? crop.GetData()?.GetCustomTextureName(DefaultCropSheetName);

                if (crop.overrideTexturePath.Value != desiredValue)
                {
                    crop.overrideTexturePath.Value = desiredValue;
                    ResetDrawnTexture(crop);
                }

                return;
            }
        }
    }

    private static void RefreshDrawnTextures(GameLocation? location)
    {
        if (location == null) return;

        ForEachHoeDirt(location, hoeDirt =>
        {
            if (hoeDirt.crop != null) ResetDrawnTexture(hoeDirt.crop);
        });
    }

    private static void ResetDrawnTexture(Crop crop)
    {
        ModEntry.ModHelper.Reflection.GetField<Texture2D?>(crop, "_drawnTexture").SetValue(null);
        crop.updateDrawMath(crop.tilePosition);
    }

    private static void ForEachHoeDirt(GameLocation location, Action<HoeDirt> action)
    {
        foreach (var terrainFeature in location.terrainFeatures.Values)
        {
            if (terrainFeature is HoeDirt hoeDirt) action(hoeDirt);
        }

        foreach (var obj in location.objects.Values)
        {
            if (obj is IndoorPot pot && pot.hoeDirt.Value != null) action(pot.hoeDirt.Value);
        }
    }
}
