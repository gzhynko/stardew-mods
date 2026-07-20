using System;
using System.Collections.Generic;
using System.Linq;
using AnimalsNeedWater.Core.Content;
using AnimalsNeedWater.Core.Content.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace AnimalsNeedWater.Core.Game;

public class TroughVisuals
{
    private Dictionary<string, Texture2D?> _overlayTexturesCache = new Dictionary<string, Texture2D?>(StringComparer.OrdinalIgnoreCase);

    public void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        foreach (var invalidated in e.NamesWithoutLocale)
        {
            _overlayTexturesCache.Remove(invalidated.BaseName);
        }
    }

    public void EnsureTileSheets(Building building)
    {
        var buildingName = building.buildingType.Value;
        if (!ModEntry.PlacementRegistry.TryGetPlacement(buildingName, out var buildingProfile))
            return;
        
        var indoorsMap = building.indoors.Value.Map;

        // trough tilesheet
        var troughSource = buildingProfile.TroughTexture
                     ?? (ModEntry.Config.CleanerTroughs
                         ? AssetManager.WaterTroughTilesheetCleanAssetName
                         : AssetManager.WaterTroughTilesheetAssetName);
        
        var existingTrough = indoorsMap.TileSheets.FirstOrDefault(ts => ts.Id == "z_waterTroughTilesheet");
        if (existingTrough == null)
        {
            var tex = ModEntry.ModHelper.GameContent.Load<Texture2D>(troughSource);
            var sheetSize =  new Size(tex.Width / 16, tex.Height / 16); // size in tiles not px
            var sheet = new TileSheet("z_waterTroughTilesheet", indoorsMap, troughSource,
                sheetSize, new Size(16, 16));
            indoorsMap.AddTileSheet(sheet);
        }
        else if (!existingTrough.ImageSource.Equals(troughSource, StringComparison.OrdinalIgnoreCase))
        {
            existingTrough.ImageSource = troughSource; // repoint the existing sheet at the new asset in case the underlying texture changed (e.g config changes, seasons)
        }

        if (buildingProfile.WateringSystemTiles.Count == 0 ||
            !ModEntry.Config.UseWateringSystems)
        {
            indoorsMap.LoadTileSheets(Game1.mapDisplayDevice); // load just the trough tilesheet 
            return;
        }

        // watering system tilesheet
        var wateringSystemSource = buildingProfile.WateringSystemTexture
                     ?? AssetManager.WateringSystemTilesheetAssetName;
        
        var existingWateringSys = indoorsMap.TileSheets.FirstOrDefault(ts => ts.Id == "z_wateringSystemTilesheet");
        if (existingWateringSys == null)
        {
            var tex = ModEntry.ModHelper.GameContent.Load<Texture2D>(wateringSystemSource);
            var sheetSize =  new Size(tex.Width / 16, tex.Height / 16); // size in tiles not px
            var sheet = new TileSheet("z_wateringSystemTilesheet", indoorsMap, wateringSystemSource,
                sheetSize, new Size(16, 16));
            indoorsMap.AddTileSheet(sheet);
        }
        else if (!existingWateringSys.ImageSource.Equals(wateringSystemSource, StringComparison.OrdinalIgnoreCase))
        {
            existingWateringSys.ImageSource = wateringSystemSource;
        }
        
        indoorsMap.LoadTileSheets(Game1.mapDisplayDevice);
    }
    
    public void ApplyTroughTiles(Building building)
    {
        var buildingName = building.buildingType.Value;
        if (!ModEntry.PlacementRegistry.TryGetPlacement(buildingName, out var buildingProfile))
            return;
        if (buildingProfile.TroughTiles.Count == 0) 
            return;

        GameLocation indoorsGameLocation = building.indoors.Value;

        foreach (TroughTile tile in buildingProfile.TroughTiles)
        {
            indoorsGameLocation.removeTile(tile.TileX, tile.TileY, tile.Layer);
        }

        Layer buildingsLayer = indoorsGameLocation.Map.GetLayer("Buildings");
        Layer frontLayer = indoorsGameLocation.Map.GetLayer("Front");
        TileSheet tilesheet = indoorsGameLocation.Map.GetTileSheet("z_waterTroughTilesheet");

        foreach (TroughTile tile in buildingProfile.TroughTiles)
        {
            var tileIndexToUse = ModEntry.TroughManager.IsWatered(building)
                ? tile.FullIndex
                : tile.EmptyIndex;
                
            if (tile.Layer!.Equals("Buildings", StringComparison.OrdinalIgnoreCase))
                buildingsLayer.Tiles[tile.TileX, tile.TileY] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: tileIndexToUse);
            else if (tile.Layer.Equals("Front", StringComparison.OrdinalIgnoreCase))
                frontLayer.Tiles[tile.TileX, tile.TileY] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: tileIndexToUse);
        }
    }
    
    public void ApplyWateringSystem(Building building)
    {
        if (!ModEntry.Config.UseWateringSystems) return;
        
        var buildingName = building.buildingType.Value;
        if (!ModEntry.PlacementRegistry.TryGetPlacement(buildingName, out var buildingProfile))
            return;
        if (buildingProfile.WateringSystemTiles.Count == 0) 
            return;
        
        GameLocation indoorsGameLocation = building.indoors.Value;

        foreach (WateringSystemTile tile in buildingProfile.WateringSystemTiles)
        {
            foreach (var tileToRemove in tile.TilesToRemove)
            {
                indoorsGameLocation.removeTile(tileToRemove.TileX, tileToRemove.TileY, tileToRemove.Layer);
            }
        }

        Layer buildingsLayer = indoorsGameLocation.Map.GetLayer("Buildings");
        Layer frontLayer = indoorsGameLocation.Map.GetLayer("Front");
        TileSheet tilesheet = indoorsGameLocation.Map.GetTileSheet("z_wateringSystemTilesheet");

        foreach (WateringSystemTile tile in buildingProfile.WateringSystemTiles)
        {
            if (tile.Layer!.Equals("Buildings", StringComparison.OrdinalIgnoreCase))
                buildingsLayer.Tiles[tile.TileX, tile.TileY] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: tile.SystemIndex);
            else if (tile.Layer.Equals("Front", StringComparison.OrdinalIgnoreCase))
                frontLayer.Tiles[tile.TileX, tile.TileY] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: tile.SystemIndex);
        }
    }

    private void EnsureExteriorOverlayCache(Building building)
    {
        var buildingName = building.buildingType.Value;
        if (!ModEntry.PlacementRegistry.TryGetPlacement(buildingName, out var buildingProfile))
            return;
        if (buildingProfile.ExteriorEmptyOverlay == null) 
            return;
        if (_overlayTexturesCache.ContainsKey(buildingProfile.ExteriorEmptyOverlay.Texture)) return;
        
        Texture2D? tex = null;
        try
        {
            tex = ModEntry.ModHelper.GameContent.Load<Texture2D>(buildingProfile.ExteriorEmptyOverlay.Texture);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.LogOnce($"Unable to load exterior overlay texture for building {buildingName}: {e}", LogLevel.Warn);
        }

        _overlayTexturesCache[buildingProfile.ExteriorEmptyOverlay.Texture] = tex;
    }

    public void ReapplyVisuals(Building building)
    {
        EnsureTileSheets(building);
        ApplyWateringSystem(building);
        ApplyTroughTiles(building);
        
        // ensure exterior overlays are cached. this reduces latency in the draw method below
        EnsureExteriorOverlayCache(building);
    }

    public void DrawExteriorOverlay(Building building, SpriteBatch spriteBatch)
    {
        if (!ModEntry.Config.ChangeBuildingTextureIfTroughIsEmpty 
            || building.GetIndoors() is not AnimalHouse
            || building.isUnderConstruction()) return; // note: this ignores upgrades
        
        var buildingName = building.buildingType.Value;
        if (!ModEntry.PlacementRegistry.TryGetPlacement(buildingName, out var buildingProfile))
            return;
        if (buildingProfile.ExteriorEmptyOverlay == null) 
            return;
        
        if (ModEntry.TroughManager.IsWatered(building))
            return;

        // should be handled by ReapplyVisuals normally
        if (!_overlayTexturesCache.ContainsKey(buildingProfile.ExteriorEmptyOverlay.Texture))
        {
            EnsureExteriorOverlayCache(building);
        }
        
        var overlayTex = _overlayTexturesCache[buildingProfile.ExteriorEmptyOverlay.Texture];
        if (overlayTex == null) return; // null means texture failed to load; cant do much about it
        
        var overlayOffsetBottomLeft = buildingProfile.ExteriorEmptyOverlay.DrawOffset.ToVector2() * 4f;
        var overlaySourceRect = buildingProfile.ExteriorEmptyOverlay.SourceRect;
        
        // mostly copy-paste from Building.draw
        var data = building.GetData();
        float num1 = (building.tileY.Value + building.tilesHigh.Value) * 64;
        float num2 = num1;
        if (data != null)
            num2 -= data.SortTileOffset * 64f;
        float layerDepth1 = num2 / 10000f + 0.0001f; // add a tiny bit of z to draw the overlay over the building
        var vector2_1 = new Vector2(building.tileX.Value * 64, building.tileY.Value * 64 + building.tilesHigh.Value * 64);
        var vector2_2 = Vector2.Zero;
        if (data != null)
            vector2_2 = data.DrawOffset * 4f;
        var origin = new Vector2(0.0f, overlaySourceRect.Height);
        spriteBatch.Draw(overlayTex, Game1.GlobalToLocal(Game1.viewport, vector2_1 + vector2_2 + overlayOffsetBottomLeft), overlaySourceRect, building.color * building.alpha, 0.0f, origin, 4f, SpriteEffects.None, layerDepth1);
    }
}