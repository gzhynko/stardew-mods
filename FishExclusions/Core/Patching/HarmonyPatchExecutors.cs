using FishExclusions.Core.Game;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

// ReSharper disable InconsistentNaming

namespace FishExclusions.Core.Patching;

internal static class HarmonyPatchExecutors
{
    private const double RecursionGuard = 909;

    /// <summary> Patch for the GameLocation.getFish method. </summary>
    public static void GetFish(GameLocation __instance, float millisecondsAfterNibble, string? bait, int waterDepth,
        Farmer who, double baitPotency, Vector2 bobberTile, ref Item? __result, string? locationName = null)
    {
        if (!ModEntry.ExclusionsEnabled) return;
        // island walnut catches can return null
        if (__result is null) return;
        if ((int)baitPotency == (int)RecursionGuard) return;
        // re-rolling a pond catch would pull extra fish out of the pond
        if (IsFishPondCatch(__instance, who, bobberTile)) return;

        var bannedIds = ExclusionResolver.GetExcludedFish(ModEntry.Config, __instance.GetSeasonKey(), __instance.Name,
            __instance.GetWeather());
        if (!ExclusionResolver.IsExcluded(__result, bannedIds)) return;

        var numAttempts = 0;
        // Retry TimesToRetry times before giving up.
        var maxAttempts = ModEntry.Config.TimesToRetry;
        var result = __result;
        while (numAttempts < maxAttempts && ExclusionResolver.IsExcluded(result, bannedIds))
        {
            // call getFish repeatedly, but with a sentinel in place of baitPotency to make sure this
            // while loop doesn't execute indefinitely
            result = __instance.getFish(millisecondsAfterNibble, bait, waterDepth, who, RecursionGuard, bobberTile,
                locationName);
            numAttempts++;
        }

        // return the fallback item in case all possible fish for this water body / season / weather is excluded
        if (result is null || ExclusionResolver.IsExcluded(result, bannedIds))
        {
            var fallbackId = ModEntry.Config.ItemToCatchIfAllFishIsExcluded is "" or "0"
                ? "(O)168" // trash
                : ModEntry.Config.ItemToCatchIfAllFishIsExcluded;
            result = ItemRegistry.Create(fallbackId);
        }

        __result = result;
    }

    /// <summary> mirrors the fish pond check at the top of GameLocation.getFish </summary>
    private static bool IsFishPondCatch(GameLocation location, Farmer who, Vector2 bobberTile)
    {
        if (bobberTile == Vector2.Zero || who.currentLocation?.NameOrUniqueName != location.NameOrUniqueName)
            return false;

        foreach (var building in location.buildings)
        {
            if (building is FishPond pond && pond.isTileFishable(bobberTile)) return true;
        }

        return false;
    }
}