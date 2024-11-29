using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace ScytheHarvestsMore.Patching;

public class HarmonyPatches
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
}