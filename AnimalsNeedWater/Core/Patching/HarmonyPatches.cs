using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

// ReSharper disable InconsistentNaming

namespace AnimalsNeedWater.Core.Patching
{
    internal class HarmonyPatches
    {
        /// <summary> Patch for the FarmAnimal.dayUpdate method. </summary>
        [HarmonyPriority(500)]
        public static void AnimalDayUpdate(FarmAnimal __instance, GameLocation environment)
        {
            try
            {
                HarmonyPatchExecutors.AnimalDayUpdateExecutor(__instance, environment);
            }
            catch (Exception e)
            {
                ModEntry.ModMonitor.Log($"Failed in { nameof(AnimalDayUpdate) }:\n{ e }", LogLevel.Error);
            }
        }

        /// <summary> Patch for the FarmAnimal.behaviors method. </summary>
        [HarmonyPriority(600)]
        public static bool AnimalBehaviors(ref bool __result, FarmAnimal __instance, GameTime time, GameLocation location)
        {
            try
            {
                return HarmonyPatchExecutors.AnimalBehaviorsExecutor(ref __result, __instance, time, location);
            }
            catch (Exception e)
            {
                ModEntry.ModMonitor.Log($"Failed in { nameof(AnimalBehaviors) }:\n{ e }", LogLevel.Error);
                
                return true;
            }
        }

        /// <summary> Patch for the GameLocation.performToolAction method. </summary>
        [HarmonyPriority(500)]
        public static bool GameLocationToolAction(Tool t, int tileX, int tileY)
        { 
            try
            {
                return HarmonyPatchExecutors.GameLocationToolActionExecutor(t, tileX, tileY);
            }
            catch (Exception e)
            {
                ModEntry.ModMonitor.Log($"Failed in { nameof(GameLocationToolAction) }:\n{ e }", LogLevel.Error);
                
                return true;
            }
        }

        /// <summary> Patch for the OnLocationChanged method. </summary>
        [HarmonyPriority(500)]
        public static void OnLocationChanged(GameLocation oldLocation, GameLocation newLocation)
        {
            try
            {
                HarmonyPatchExecutors.OnLocationChangedExecutor(oldLocation, newLocation);
            }
            catch (Exception e)
            {
                ModEntry.ModMonitor.Log($"Failed in { nameof(OnLocationChanged) }:\n{ e }", LogLevel.Error);
            }
        }
    }
}
