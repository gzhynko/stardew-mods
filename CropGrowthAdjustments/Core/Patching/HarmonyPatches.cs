using System;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

// ReSharper disable InconsistentNaming

namespace CropGrowthAdjustments.Core.Patching;

internal static class HarmonyPatches
{
    public static void HoeDirtPlant(HoeDirt __instance, ref bool __result, string itemId, Farmer who, bool isFertilizer)
    {
        try
        {
            HarmonyPatchExecutors.HoeDirtPlant(__instance, ref __result);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in {nameof(HoeDirtPlant)}:\n{e}", LogLevel.Error);
        }
    }
    
    public static bool CropNewDay(Crop __instance, int state)
    {
        try
        {
            return HarmonyPatchExecutors.CropNewDay(__instance, state);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in {nameof(CropNewDay)}:\n{e}", LogLevel.Error);
            
            // run the original method on patch fail
            return true;
        }
    }
}