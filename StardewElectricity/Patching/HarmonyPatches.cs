using Microsoft.Xna.Framework;
using StardewElectricity.Buildings;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

// ReSharper disable InconsistentNaming

namespace StardewElectricity.Patching
{
    public class HarmonyPatches
    {
        /// <summary> Patch for the CarpenterMenu.setNewActiveBlueprint method. </summary>
        public static void SetNewActiveBlueprint(CarpenterMenu __instance)
        {
            if (__instance.CurrentBlueprint.name.Contains(UtilityPole.BlueprintName))
            {
                ModEntry.Instance.Helper.Reflection.GetField<Building>(__instance, "currentBuilding").SetValue(new UtilityPole(__instance.CurrentBlueprint, Vector2.Zero));
            }
        }
        
        /// <summary> Patch for the CarpenterMenu.tryToBuild method. </summary>
        public static bool TryToBuild(CarpenterMenu __instance, ref bool __result)
        {
            if (__instance.CurrentBlueprint.name.Contains(UtilityPole.BlueprintName))
            {
                __result = UtilityPole.Build((Farm)Game1.getLocationFromName("Farm"), new Vector2((Game1.viewport.X + Game1.getOldMouseX(false)) / 64f, (Game1.viewport.Y + Game1.getOldMouseY(false)) / 64f), Game1.player);
                
                return false;
            }

            return true;
        }
    }
}