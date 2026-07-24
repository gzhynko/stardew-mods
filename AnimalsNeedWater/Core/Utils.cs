using System;
using AnimalsNeedWater.Core.Content.Models;
using AnimalsNeedWater.Core.Models;
using StardewValley;
using StardewValley.Buildings;
using xTile;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace AnimalsNeedWater.Core;

public static class Utils
{
    public static void EmptyWaterBowlObject(Object waterBowl)
    {
        waterBowl.modData[ModConstants.WaterBowlItemModDataIsFullField] = "false";
        waterBowl.ParentSheetIndex = 0;
    }
    
    public static void FillWaterBowlObject(Object waterBowl)
    {
        waterBowl.modData[ModConstants.WaterBowlItemModDataIsFullField] = "true";
        waterBowl.ParentSheetIndex = 1;
    }
    
    public static Layer? ResolveLayer(Map map, string? layerId)
    {
        if (string.IsNullOrWhiteSpace(layerId)) return null;
        foreach (var layer in map.Layers)
            if (string.Equals(layer.Id, layerId, StringComparison.OrdinalIgnoreCase))
                return layer;
        return null;
    }

    public static bool InBounds(Layer layer, int x, int y)
        => x >= 0 && y >= 0 && x < layer.LayerWidth && y < layer.LayerHeight;
}