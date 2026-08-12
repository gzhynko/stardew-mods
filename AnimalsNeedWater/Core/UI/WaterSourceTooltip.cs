using System.Collections.Generic;
using AnimalsNeedWater.Core.Content.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace AnimalsNeedWater.Core.UI;

public static class WaterSourceTooltip
{
    /// <summary>
    /// Decides which i18n key (if any) should be shown as a tooltip for the given world tile.
    /// Pure logic, no drawing — safe to call outside of a render context.
    /// </summary>
    public static string? TryGetTooltipKey(GameLocation location, int tileX, int tileY)
    {
        if (location is AnimalHouse && location.ParentBuilding is Building building)
        {
            var buildingType = building.buildingType.Value;
            if (ModEntry.PlacementRegistry.TryGetPlacement(buildingType, out var placement))
            {
                bool matchesTrough = MatchesAnyTroughTile(placement.TroughTiles, tileX, tileY);
                bool matchesWateringSystem = ModEntry.Config.UseWateringSystems
                    && MatchesAnyWateringSystemTile(placement.WateringSystemTiles, tileX, tileY);

                if (matchesTrough || matchesWateringSystem)
                {
                    bool watered = ModEntry.BuildingWaterManager.IsWatered(building);
                    return watered ? "Tooltip.Building.Watered" : "Tooltip.Building.NotWatered";
                }
            }
        }

        if (location.Objects.TryGetValue(new Vector2(tileX, tileY), out Object? obj)
            && ModEntry.WaterSources.TryGet(obj, out var source))
        {
            bool full = ModEntry.WaterSources.IsFull(obj, source);
            return full ? "Tooltip.WaterSource.Full" : "Tooltip.WaterSource.Empty";
        }

        return null;
    }

    private static bool MatchesAnyTroughTile(List<TroughTile> tiles, int x, int y)
    {
        foreach (var t in tiles)
        {
            if (t.TileX == x && t.TileY == y) return true;
        }
        return false;
    }

    private static bool MatchesAnyWateringSystemTile(List<WateringSystemTile> tiles, int x, int y)
    {
        foreach (var t in tiles)
        {
            if (t.TileX == x && t.TileY == y) return true;
        }
        return false;
    }

    /// <summary> Draws the tooltip for the tile under the cursor, if any. Hooked to Display.RenderedHud. </summary>
    public static void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!ModEntry.Config.ShowWaterSourceTooltip) return;
        if (!Context.IsWorldReady) return;
        if (Game1.activeClickableMenu != null) return;
        if (Game1.currentLocation == null) return;

        var cursorTile = Game1.currentCursorTile;
        var key = TryGetTooltipKey(Game1.currentLocation, (int)cursorTile.X, (int)cursorTile.Y);
        if (key == null) return;

        var text = ModEntry.ModHelper.Translation.Get(key);
        IClickableMenu.drawHoverText(e.SpriteBatch, text, Game1.smallFont);
    }
}
