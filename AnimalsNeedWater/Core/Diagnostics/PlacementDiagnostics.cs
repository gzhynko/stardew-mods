using System.Collections.Generic;
using System.Linq;
using AnimalsNeedWater.Core.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;

namespace AnimalsNeedWater.Core.Diagnostics;

public class PlacementDiagnostics
{
    private Color _troughTileOverlayColor = Color.Purple;
    private Color _wateringSystemTileOverlayColor = Color.Blue;
    
    private Texture2D? _debugOverlayPixelTex;
    
    private bool _debugOverlayEnabled;
    private HashSet<string> _validateReportedBuildings = new HashSet<string>();
    
    public void ToggleDebugOverlay()
    {
        _debugOverlayEnabled = !_debugOverlayEnabled;
    }
    
    public void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(name => name.IsEquivalentTo(AssetManager.TroughPlacementsAssetName)))
        {
            _validateReportedBuildings.Clear();
        }
    }
    
    public void ValidateAndReport(Building building)
    {
        var indoors = building.GetIndoors();
        if (indoors is not AnimalHouse indoorsAnimalHouse) 
            return;
        
        var buildingName = building.buildingType.Value;
        if (!_validateReportedBuildings.Add(buildingName)) 
            return;

        if (!ModEntry.PlacementRegistry.TryGetPlacement(buildingName, out var buildingProfile))
        {
            if (indoorsAnimalHouse.animalsThatLiveHere.Count > 0)
            {
                // animals live here but no profile
                ModEntry.ModMonitor.Log($"Building {buildingName} has animals but does not have an ANW placement profile. Troughs will not work in this building.", LogLevel.Debug);
            }
            return;
        }

        var map = indoors.Map;

        var troughSheet = map.GetTileSheet("z_waterTroughTilesheet");
        int troughFrames = troughSheet != null ? troughSheet.SheetSize.Width * troughSheet.SheetSize.Height : -1;
        foreach (var t in buildingProfile.TroughTiles)
            ValidateTile(buildingName, map, t.Layer, t.TileX, t.TileY, new[] { t.EmptyIndex, t.FullIndex }, troughFrames, "trough");

        var wateringSysSheet = map.GetTileSheet("z_wateringSystemTilesheet");
        int wateringSysFrames = wateringSysSheet != null ? wateringSysSheet.SheetSize.Width * wateringSysSheet.SheetSize.Height : -1;
        foreach (var ws in buildingProfile.WateringSystemTiles)
            ValidateTile(buildingName, map, ws.Layer, ws.TileX, ws.TileY, new[] { ws.SystemIndex }, wateringSysFrames, "watering system");
    }
    
    private void ValidateTile(string buildingName, xTile.Map map, string? layerName,
        int x, int y, int[] tilesheetIndices, int frameCount, string kind)
    {
        var l = layerName?.ToLowerInvariant();

        var layer = l switch
        {
            "back"      => map.GetLayer("Back"),
            "buildings" => map.GetLayer("Buildings"),
            "front"     => map.GetLayer("Front"),
            _           => null
        };

        if (layer == null)
        {
            bool isKnownLayer = l is "back" or "buildings" or "front";
            ModEntry.ModMonitor.Log(isKnownLayer
                    ? $"{buildingName}: map has no '{layerName}' layer for {kind} tile ({x},{y})."
                    : $"{buildingName}: {kind} tile ({x},{y}) uses unsupported layer '{layerName}'. Use Back, Buildings, or Front",
                LogLevel.Warn);
            return;
        }

        if (x < 0 || y < 0 || x >= layer.LayerWidth || y >= layer.LayerHeight)
        {
            ModEntry.ModMonitor.Log(
                $"{buildingName}: {kind} tile ({x},{y}) is outside the map ({layer.LayerWidth}x{layer.LayerHeight})",
                LogLevel.Warn);
        }

        if (frameCount >= 0)
        {
            foreach (var idx in tilesheetIndices)
            {
                if (idx < 0 || idx >= frameCount)
                {
                    ModEntry.ModMonitor.Log(
                        $"{buildingName}: {kind} tile ({x},{y}) index {idx} is out of tilesheet range [0,{frameCount})",
                        LogLevel.Warn);
                }
            }
        }
    }


    public void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (!_debugOverlayEnabled || !Context.IsWorldReady) return;
        
        var currentBuilding = Game1.currentLocation?.ParentBuilding;
        if (currentBuilding == null ||
            !ModEntry.PlacementRegistry.TryGetPlacement(currentBuilding.buildingType.Value, out var buildingProfile))
        {
            return;
        }

        _debugOverlayPixelTex ??= MakePixel();
        
        foreach (var tile in buildingProfile.TroughTiles)
        {
            DrawTileOverlayWithCenterText(e.SpriteBatch, _troughTileOverlayColor, tile.TileX, tile.TileY, $"e: {tile.EmptyIndex}\nf: {tile.FullIndex}");
        }
        
        foreach (var tile in buildingProfile.WateringSystemTiles)
        {
            DrawTileOverlayWithCenterText(e.SpriteBatch, _wateringSystemTileOverlayColor, tile.TileX, tile.TileY, $"i: {tile.SystemIndex}");
        }
    }

    private void DrawTileOverlayWithCenterText(SpriteBatch b, Color baseColor, int tileX, int tileY, string text)
    {
        var tileWorldPos = new Vector2(tileX * 64, tileY * 64);
        var tileDrawPos = Game1.GlobalToLocal(Game1.viewport, tileWorldPos);
        b.Draw(_debugOverlayPixelTex, tileDrawPos, null, baseColor * 0.5f, 0.0f, Vector2.Zero, new Vector2(64, 64), SpriteEffects.None, 0.0f);

        var font = Game1.smallFont;
        var textSize = font.MeasureString(text);
        var textWorldPos = new Vector2(tileWorldPos.X + (64 - textSize.X) / 2, tileWorldPos.Y + (64 - textSize.Y) / 2);
        var textDrawPos = Game1.GlobalToLocal(Game1.viewport, textWorldPos);
        b.DrawString(font, text, textDrawPos, Color.White);
    }
    
    private Texture2D MakePixel()
    {
        var tex = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
        tex.SetData(new [] { Color.White });
        return tex;
    }
}