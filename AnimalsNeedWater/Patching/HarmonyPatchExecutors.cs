using System;
using System.Linq;
using AnimalsNeedWater.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Pathfinding;
using StardewValley.Tools;
using xTile.Layers;
using xTile.Tiles;
// ReSharper disable InconsistentNaming

namespace AnimalsNeedWater.Patching
{
    public static class HarmonyPatchExecutors
    {
        public static void AnimalDayUpdateExecutor(FarmAnimal __instance, GameLocation environment)
        {
            if (__instance.home != null &&
                !((AnimalHouse) __instance.home.indoors.Value).animals.ContainsKey(__instance.myID.Value) &&
                environment is not AnimalHouse && !__instance.home.animalDoorOpen.Value) return;
            
            // check whether BuildingsWithWateredTrough contains the building the animal lives in and whether it was able to drink outside or not
            if ((__instance.home != null 
                 && ModEntry.Data.BuildingsWithWateredTrough.Contains(__instance.home.indoors.Value.NameOrUniqueName.ToLower())) 
                || ModEntry.Data.IsAnimalFull(__instance))
            {
                // increase friendship points
                __instance.friendshipTowardFarmer.Value += Math.Abs(ModEntry.Config.FriendshipPointsForWateredTrough);
            }
        }
        
        public static bool AnimalBehaviorsExecutor(ref bool __result, FarmAnimal __instance, GameTime time, GameLocation location)
        {
            if (__instance.home == null)
            {
                __result = false;
                return false;
            }
            if (!Game1.IsMasterGame) // do not run if not host
            {
                __result = false;
                return false;
            }
            if (__instance.controller != null) // do not run if has other pathfinding in progress
            {
                __result = true;
                return true;
            }
            
            if (ModEntry.Config.AnimalsCanDrinkOutside 
                && !__instance.isSwimming.Value 
                && location.IsOutdoors 
                && !ModEntry.Data.IsAnimalFull(__instance) 
                && Game1.random.NextDouble() < 0.002 // set a random chance of 0.2% each frame to pathfind
                && FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick)
            {
                // pathfind to the closest water tile
                ++FarmAnimal.NumPathfindingThisTick;
                __instance.controller = new PathFindController(__instance, location, WaterEndPointFunction, -1, BehaviorAfterFindingWater, 200, Point.Zero);
            }

            __result = true;
            return true;
        }
        
        /// <summary> Search for water tiles. </summary>
        private static bool WaterEndPointFunction(
            PathNode currentPoint,
            Point endPoint,
            GameLocation location,
            Character c) 
        {
            if (!ModEntry.Config.AnimalsCanOnlyDrinkFromWaterBodies)
            {
                // check four adjacent tiles for wells, fish ponds, etc.
                return location.CanRefillWateringCanOnTile(currentPoint.x - 1, currentPoint.y) 
                       || location.CanRefillWateringCanOnTile(currentPoint.x, currentPoint.y - 1) 
                       || location.CanRefillWateringCanOnTile(currentPoint.x, currentPoint.y + 1) 
                       || location.CanRefillWateringCanOnTile(currentPoint.x + 1, currentPoint.y);
            }
            
            // check four adjacent tiles for open water (no wells, fish ponds, etc.)
            return location.isOpenWater(currentPoint.x - 1, currentPoint.y) 
                   || location.isOpenWater(currentPoint.x, currentPoint.y - 1) 
                   || location.isOpenWater(currentPoint.x, currentPoint.y + 1) 
                   || location.isOpenWater(currentPoint.x + 1, currentPoint.y);
        }

        /// <summary> Animal behavior after finding a water tile and pathfinding to it. </summary>
        private static void BehaviorAfterFindingWater(Character c, GameLocation environment)
        {
            // return if the animal is already on the list
            if (ModEntry.Data.IsAnimalFull((c as FarmAnimal)!))
                return;
            
            // do the 'happy' emote and add the animal to the Full Animals list
            c.doEmote(32);
            ((FarmAnimal)c).isEating.Value = true; // do the eating animation
            ModEntry.Data.AddFullAnimal((c as FarmAnimal)!);
        }
        
        public static bool GameLocationToolActionExecutor(Tool tool, int tileX, int tileY)
        {
            GameLocation currentLocation = Game1.currentLocation;
            var building = currentLocation.ParentBuilding;
            // execute original method if not in a building
            if (building == null)
                return true;
            var buildingUniqueName = currentLocation.NameOrUniqueName;
            var buildingNameNoUnique = building.buildingType.Value;
            
            // execute original method if this is not a watering can
            if (!(tool is WateringCan can) || can.WaterLeft <= 0) 
                return true;
            
            // check if hit a water bowl object
            var objectHit = currentLocation.getObjectAtTile(tileX, tileY);
            // hit a bowl!
            if (objectHit != null && objectHit.HasTypeId("(BC)") && objectHit.ItemId == ModConstants.WaterBowlItemId)
            {
                Utility.FillWaterBowlObject(objectHit);
                return false;
            }

            // if did not hit any water bowl objects, check for trough tiles hits instead
            
            // skip if the building is watered
            if (ModEntry.Data.BuildingsWithWateredTrough.Contains(buildingUniqueName.ToLower()))
                return true;
            
            var buildingProfile = ModEntry.GetProfileForBuilding(buildingNameNoUnique);
            // execute original method if this building should not be affected by this mod
            if (buildingProfile == null)
                return true;

            var profilePlacement = buildingProfile.GetPlacementForBuildingName(buildingNameNoUnique);
            var troughHit = profilePlacement.TroughTiles.Any(troughTile =>
                tileX == troughTile.TileX && tileY == troughTile.TileY);
            // check if hit any water trough tiles
            if (!troughHit)
                return true;
            
            ModEntry.Data.BuildingsWithWateredTrough.Add(buildingUniqueName.ToLower());
            ModEntry.SendTroughWateredMessage(buildingUniqueName.ToLower());
            UpdateBuildingTroughs(building);
        
            // change building texture to reflect full trough
            switch (buildingNameNoUnique.ToLower())
            {
                case "coop":
                    ModEntry.ChangeCoopTexture(building, false);
                    break;
                    
                case "big coop":
                    ModEntry.ChangeBigCoopTexture(building, false);
                    break;
            }
            
            // give additional friendship to animals currently here
            foreach (FarmAnimal animal in currentLocation.animals.Values)
            {
                if (ModEntry.Config.ShowLoveBubblesOverAnimalsWhenWateredTrough)
                {
                    animal.doEmote(ModConstants.LoveEmote);
                }

                animal.friendshipTowardFarmer.Value += Math.Abs(ModEntry.Config
                    .AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding);
            }

            // skip original method
            return false;
        }
        
        public static void OnLocationChangedExecutor(GameLocation _oldLocation, GameLocation newLocation)
        {
            Building building = newLocation.ParentBuilding;
            if (building == null)
                return;
            
            UpdateBuildingTroughs(building);
        }

        public static void UpdateBuildingTroughs(Building building)
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
    }
}
