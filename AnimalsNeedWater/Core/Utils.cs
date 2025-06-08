using System;
using AnimalsNeedWater.Core.Content.Models;
using AnimalsNeedWater.Core.Models;
using StardewValley;
using StardewValley.Buildings;
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
    
    public static void UpdateBuildingTroughTiles(Building building)
    {
        var buildingNameNoUnique = building.buildingType.Value;
        var buildingProfile = ModEntry.GetProfileForBuilding(buildingNameNoUnique);
        if (buildingProfile == null)
            return;

        var buildingUniqueName = building.GetIndoorsName();
        var profilePlacement = buildingProfile.GetPlacementForBuildingName(buildingNameNoUnique);
            
        GameLocation indoorsGameLocation = building.indoors.Value;

        foreach (TroughTile tile in profilePlacement.TroughTiles)
        {
            indoorsGameLocation.removeTile(tile.TileX, tile.TileY, tile.Layer);
        }

        Layer buildingsLayer = indoorsGameLocation.Map.GetLayer("Buildings");
        Layer frontLayer = indoorsGameLocation.Map.GetLayer("Front");
        TileSheet tilesheet = indoorsGameLocation.Map.GetTileSheet("z_waterTroughTilesheet");

        foreach (TroughTile tile in profilePlacement.TroughTiles)
        {
            var tileIndexToUse = ModEntry.Data.BuildingsWithWateredTrough.Contains(buildingUniqueName.ToLower())
                ? tile.FullIndex
                : tile.EmptyIndex;
                
            if (tile.Layer!.Equals("Buildings", StringComparison.OrdinalIgnoreCase))
                buildingsLayer.Tiles[tile.TileX, tile.TileY] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: tileIndexToUse);
            else if (tile.Layer.Equals("Front", StringComparison.OrdinalIgnoreCase))
                frontLayer.Tiles[tile.TileX, tile.TileY] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: tileIndexToUse);
        }
    }

    public static TroughPlacementConfiguration TroughPlacementRetainSelectBuildings(
        TroughPlacementConfiguration placement, string[] buildings)
    {
        
    }
}