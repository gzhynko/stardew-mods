using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

// ReSharper disable InconsistentNaming

namespace ScytheHarvestsMore.Core.Patching;

internal static class HarmonyPatches
{
    public static void FruitTreePerformToolAction(FruitTree __instance, Tool t, int explosion, Vector2 tileLocation)
    {
        try
        {
            HarmonyPatchExecutors.FruitTreePerformToolAction(__instance, t, explosion, tileLocation);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in { nameof(FruitTreePerformToolAction) }:\n{ e }", LogLevel.Error);
        }
    }
    
    public static void ObjectPerformToolAction(Object __instance, Tool t)
    {
        try
        {
            HarmonyPatchExecutors.ObjectPerformToolAction(__instance, t);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in { nameof(ObjectPerformToolAction) }:\n{ e }", LogLevel.Error);
        }
    }
    
    public static void HoeDirtPerformToolAction(HoeDirt __instance, Tool t, int damage, Vector2 tileLocation)
    {
        try
        {
            HarmonyPatchExecutors.HoeDirtPerformToolAction(__instance, t, tileLocation);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in { nameof(HoeDirtPerformToolAction) }:\n{ e }", LogLevel.Error);
        }
    }

    public static void BushPerformToolAction(Bush __instance, Tool t, int explosion, Vector2 tileLocation)
    {
        try
        {
            HarmonyPatchExecutors.BushPerformToolAction(__instance, t, tileLocation);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in { nameof(BushPerformToolAction) }:\n{ e }", LogLevel.Error);
        }
    }
    
    public static void TreePerformToolAction(Tree __instance, Tool t, int explosion, Vector2 tileLocation)
    {
        try
        {
            HarmonyPatchExecutors.TreePerformToolAction(__instance, t, tileLocation);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in { nameof(TreePerformToolAction) }:\n{ e }", LogLevel.Error);
        }
    }
}