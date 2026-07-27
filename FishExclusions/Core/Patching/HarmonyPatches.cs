using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

// ReSharper disable InconsistentNaming

namespace FishExclusions.Core.Patching;

internal static class HarmonyPatches
{
    public static void GetFish(GameLocation __instance, float millisecondsAfterNibble, string? bait, int waterDepth,
        Farmer who, double baitPotency, Vector2 bobberTile, ref Item? __result, string? locationName = null)
    {
        try
        {
            HarmonyPatchExecutors.GetFish(__instance, millisecondsAfterNibble, bait, waterDepth, who, baitPotency,
                bobberTile, ref __result, locationName);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in {nameof(GetFish)}:\n{e}", LogLevel.Error);
        }
    }
}