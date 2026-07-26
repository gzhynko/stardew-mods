using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace AnimalsNeedWater.Core.Game;

public class OutsideDrinkingSpotCache
{
    // keyed by location name, lazy filled; null = not computed yet
    private readonly Dictionary<string, bool?[,]> _cache = new(StringComparer.OrdinalIgnoreCase);

    public void Invalidate() => _cache.Clear();

    public void OnBuildingListChanged(object? sender, BuildingListChangedEventArgs e)
    {
        Invalidate();
    }
    
    public void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        // guard against e.g moving a well thru robin's menu
        if (e.OldMenu is CarpenterMenu)
        {
            Invalidate();
        }
    }
    
    public void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
    {
        // only isOpenWater is object-sensitive right now: returns false for a water tile that has an object on it (like a crab pot);
        // CanRefillWateringCanOnTile seemns to ignore objects right now, so dont invalidate or do a Concat+linear scan in the common case.
        if (!ModEntry.Config.AnimalsCanOnlyDrinkFromWaterBodies) return;

        // for now just invalidate if e.g. placed down a crab pot
        if (e.Added.Concat(e.Removed).Any(p => e.Location.isWaterTile((int)p.Key.X, (int)p.Key.Y)))
            Invalidate();
    }

    public bool IsDrinkingSpot(GameLocation location, int x, int y)
    {
        var layer = location.Map.Layers[0];
        if (!_cache.TryGetValue(location.NameOrUniqueName, out var grid))
        {
            grid = new bool?[layer.LayerWidth, layer.LayerHeight];
            _cache[location.NameOrUniqueName] = grid;
        }
        
        // check the bounds after we know the grid; in case something edits a map mid-day and changes layer dimensions, we dont want to NRE
        if (x < 0 || y < 0 || x >= grid.GetLength(0) || y >= grid.GetLength(1))
            return false;

        var cached = grid[x, y];
        if (cached.HasValue) return cached.Value;

        var result = Compute(location, x, y);
        grid[x, y] = result;
        return result;
    }

    private static bool Compute(GameLocation location, int x, int y)
    {
        if (!ModEntry.Config.AnimalsCanOnlyDrinkFromWaterBodies)
        {
            return location.CanRefillWateringCanOnTile(x - 1, y)
                   || location.CanRefillWateringCanOnTile(x, y - 1)
                   || location.CanRefillWateringCanOnTile(x, y + 1)
                   || location.CanRefillWateringCanOnTile(x + 1, y);
        }

        return location.isOpenWater(x - 1, y)
               || location.isOpenWater(x, y - 1)
               || location.isOpenWater(x, y + 1)
               || location.isOpenWater(x + 1, y);
    }
}