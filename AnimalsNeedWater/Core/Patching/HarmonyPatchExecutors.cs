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
        
        var settings = ModEntry.AnimalSettings.For(__instance);
        if (!settings.NeedsWater) return;

        var home = __instance.home;

        if (home?.GetIndoors() is AnimalHouse animalHouse
            && !animalHouse.animals.ContainsKey(__instance.myID.Value)
            && environment is not AnimalHouse
            && !home.animalDoorOpen.Value)
            return;

        // check whether the building the animal lives in is watered (or has a full water bowl)
        // and whether the animal was able to drink outside or not
        bool watered = home?.GetIndoors() is AnimalHouse && ModEntry.BuildingWaterManager.IsWatered(home);
        if (watered || ModEntry.Data.IsAnimalFull(__instance)) 
        {
            // increase friendship points
            ModEntry.AnimalFriendship.AddDailyWaterFriendship(__instance);
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
            var settings = ModEntry.AnimalSettings.For(__instance);
            if (settings.NeedsWater && settings.CanDrinkOutside)
            {
                // pathfind to the closest water tile
                ++FarmAnimal.NumPathfindingThisTick;
                __instance.controller = new PathFindController(__instance, location, WaterEndPointFunction, -1,
                    BehaviorAfterFindingWater, 200, Point.Zero);
            }
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
        return ModEntry.OutsideDrinkingSpots.IsDrinkingSpot(location, currentPoint.x, currentPoint.y);
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
    
    public static void AnimalGetMoodMessagePostfix(ref string __result, FarmAnimal __instance)
    {
        if (!ModEntry.Config.ShowMoodThirstyStatus) return;
        if (!ModEntry.AnimalSettings.For(__instance).NeedsWater) return;

        var statusLine = GetWaterStatusLine(__instance);
        if (statusLine != null)
            __result += "\n" + statusLine;
    }

    private static string? GetWaterStatusLine(FarmAnimal animal)
    {
        // no water available right now; actionable
        if (!ModEntry.ThirstTracker.HasWaterAccess(animal))
        {
            var lastWateredDay = ModEntry.ThirstTracker.ReadLastWateredDay(animal);
            // unseeded animals have no meaningful history
            if (lastWateredDay != int.MinValue)
            {
                var daysWithoutWater = Game1.Date.TotalDays - lastWateredDay;
                var i18nKey = ModEntry.ThirstTracker.IntervalDue(animal)
                    ? daysWithoutWater == 1 ? "Animal.Status.Thirsty.today" : "Animal.Status.Thirsty.multipleDays"
                    : "Animal.Status.NoWaterButNotThirstyYet";

                return ModEntry.ModHelper.Translation.Get(i18nKey, new { days = daysWithoutWater });
            }
        }

        // has water now, but lost friendship last night; explain why
        if (ModEntry.ThirstTracker.WasAnimalLeftThirstyYesterday(animal))
            return ModEntry.ModHelper.Translation.Get("Animal.Status.WasLeftThirsty");

        return null;
    }
    
    private static void AwardWateringBonusInLocation(GameLocation location)
    {
        foreach (FarmAnimal animal in location.animals.Values)
        {
            var settings = ModEntry.AnimalSettings.For(animal);
            if (!settings.NeedsWater) continue;

            if (ModEntry.Config.ShowLoveBubblesOverAnimalsWhenWateredTrough)
            {
                ModEntry.AnimalEmotes.EmoteBonus(animal);
            }

            ModEntry.AnimalFriendship.AddWitnessedWaterFriendship(animal);
        }
    }
    
    private static bool TryAwardWateringBonus(Building building, GameLocation location)
    {
        var name = building.GetIndoorsName();
        if (ModEntry.Data.BuildingsWithBonusAwardedToday.Contains(name))
            return false;
        
        ModEntry.BuildingWaterManager.MarkBonusAwardedName(name);
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
        if (objectHit != null && ModEntry.WaterSources.TryGet(objectHit, out var source))
        {
            // not fillable by can: let the original method run
            if (!source.FillableByCan)
                return true;

            if (ModEntry.WaterSources.IsFull(objectHit, source))
                // permanent full: run original
                // refillable full: skip
                return !source.NeedsRefilling;

            ModEntry.WaterSources.Fill(objectHit, source);
            location.playSound("slosh");
            TryAwardWateringBonus(building, location); // give additional friendship to animals currently here
                
            return true;
        }

        // if did not hit any water bowl objects, check for trough tiles hits instead

        // skip if the building's troughs are watered
        if (ModEntry.BuildingWaterManager.BuildingHasTroughsWatered(building))
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
        
        ModEntry.BuildingWaterManager.MarkWatered(building);
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