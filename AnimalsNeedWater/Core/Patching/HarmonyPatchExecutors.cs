using System;
using System.Linq;
using AnimalsNeedWater.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Pathfinding;
using StardewValley.Tools;
using xTile.Layers;
using xTile.Tiles;

// ReSharper disable InconsistentNaming

namespace AnimalsNeedWater.Core.Patching;

public static class HarmonyPatchExecutors
{

    public static void AnimalDayUpdateExecutor(FarmAnimal __instance, GameLocation environment)
    {
        if (!Context.IsMainPlayer) return;
        
        var home = __instance.home;

        if (home?.GetIndoors() is AnimalHouse animalHouse
            && !animalHouse.animals.ContainsKey(__instance.myID.Value)
            && environment is not AnimalHouse
            && !home.animalDoorOpen.Value)
            return;

        // check whether the building the animal lives in is watered (or has a full water bowl)
        // and whether the animal was able to drink outside or not
        bool watered = home?.GetIndoors() is AnimalHouse && ModEntry.TroughManager.IsWatered(home);
        if (watered || ModEntry.Data.IsAnimalFull(__instance)) 
        {
            // increase friendship points
            __instance.friendshipTowardFarmer.Value += Math.Abs(ModEntry.Config.FriendshipPointsForWateredTrough);
            __instance.friendshipTowardFarmer.Value = Math.Clamp(__instance.friendshipTowardFarmer.Value, 0, 1000);
        }
    }

    public static bool AnimalBehaviorsExecutor(ref bool __result, FarmAnimal __instance, GameTime time,
        GameLocation location)
    {
        if (__instance.home == null)
        {
            __result = false;
            return false;
        }

        if (!Context.IsMainPlayer) // do not run if not host
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
            __instance.controller = new PathFindController(__instance, location, WaterEndPointFunction, -1,
                BehaviorAfterFindingWater, 200, Point.Zero);
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
        if (!Context.IsMainPlayer) return;
        
        var animal = (c as FarmAnimal)!;
        // return if the animal is already on the list
        if (ModEntry.Data.IsAnimalFull(animal))
            return;

        // do the 'happy' emote and add the animal to the Full Animals list
        ModEntry.AnimalEmotes.EmoteDrankOutside(animal);
        
        animal.isEating.Value = true; // do the eating animation
        
        ModEntry.ThirstTracker.AddFullAnimal(animal.myID.Value);
        ModEntry.MessageBridge.SendAddFullAnimalMessage(animal);
    }

    private static void AwardWateringBonusInLocation(GameLocation location)
    {
        foreach (FarmAnimal animal in location.animals.Values)
        {
            if (ModEntry.Config.ShowLoveBubblesOverAnimalsWhenWateredTrough)
            {
                ModEntry.AnimalEmotes.EmoteBonus(animal);
            }

            animal.friendshipTowardFarmer.Value += Math.Abs(ModEntry.Config
                .AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding);
            animal.friendshipTowardFarmer.Value = Math.Clamp(animal.friendshipTowardFarmer.Value, 0, 1000);
        }
    }
    
    private static bool TryAwardWateringBonus(Building building, GameLocation location)
    {
        var name = building.GetIndoorsName();
        if (ModEntry.Data.BuildingsWithBonusAwardedToday.Contains(name))
            return false;
        
        ModEntry.TroughManager.MarkBonusAwardedName(name);
        ModEntry.MessageBridge.SendBonusAwardedMessage(name);
        AwardWateringBonusInLocation(location);
        return true;
    }


    public static bool GameLocationToolActionExecutor(GameLocation location, Tool tool, int tileX, int tileY)
    {
        var building = location.ParentBuilding;
        // execute original method if not in a building
        if (building == null)
            return true;

        var buildingName = building.buildingType.Value;

        // execute original method if this is not a watering can
        if (!(tool is WateringCan can) || can.WaterLeft <= 0)
            return true;

        // check if hit a water bowl object
        var objectHit = location.getObjectAtTile(tileX, tileY);
        // hit a bowl!
        if (objectHit != null && objectHit.HasTypeId("(BC)") && objectHit.ItemId == ModConstants.WaterBowlItemId)
        {
            bool wasThisFull = objectHit.modData.TryGetValue(ModConstants.WaterBowlItemModDataIsFullField, out var v) && v == "true";
            if (!wasThisFull)
            {
                Utils.FillWaterBowlObject(objectHit);
                location.playSound("slosh");
                TryAwardWateringBonus(building, location); // give additional friendship to animals currently here
            }

            return !wasThisFull;
        }

        // if did not hit any water bowl objects, check for trough tiles hits instead

        // skip if the building's troughs are watered
        if (ModEntry.TroughManager.BuildingHasTroughsWatered(building))
            return true;

        // execute original method if this building should not be affected by this mod
        if (!ModEntry.PlacementRegistry.TryGetPlacement(buildingName, out var buildingProfile))
            return true;

        var troughHit = buildingProfile.TroughTiles.Any(troughTile =>
            tileX == troughTile.TileX && tileY == troughTile.TileY);
        // check if hit any water trough tiles
        if (!troughHit)
            return true;

        location.playSound("slosh");
        
        ModEntry.TroughManager.MarkWatered(building);
        ModEntry.MessageBridge.SendTroughWateredMessage(building);
        ModEntry.TroughVisuals.ApplyTroughTiles(building);
        
        TryAwardWateringBonus(building, location); // give additional friendship to animals currently here
        
        // skip original method
        return false;
    }

    public static void BuildingDrawPostfix(Building building, SpriteBatch spriteBatch)
    {
        ModEntry.TroughVisuals.DrawExteriorOverlay(building, spriteBatch);
    }
}