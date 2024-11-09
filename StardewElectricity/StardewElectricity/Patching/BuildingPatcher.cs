using Common.Patching;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
// ReSharper disable InconsistentNaming

namespace StardewElectricity.Patching;

public class BuildingPatcher : BasePatcher
{
    public override void Apply(Harmony harmony, IMonitor modMonitor)
    {
        harmony.Patch(
            AccessTools.Method(typeof(Building), nameof(Building.drawShadow)),
            prefix: new HarmonyMethod(typeof(BuildingPatcher), nameof(BuildingPatcher.DrawShadow_Prefix))
        );
    }
    
    private static bool DrawShadow_Prefix(Building __instance, SpriteBatch b, int localX = -1, int localY = -1)
    {
        // apply shadow changes to the utility pole building
        if (__instance.buildingType.Value != Utility.Constants.UtilityPoleBuildingTypeName)
            return true; // run original method
            
        Rectangle rectangle = __instance.getSourceRectForMenu() ?? __instance.getSourceRect();
        Vector2 offset = new Vector2(8.0f * 4.0f, -8.0f * 4.0f);
        Vector2 position = 
            localX == -1 ? Game1.GlobalToLocal(new Vector2(__instance.tileX.Value * 16 * 4, (__instance.tileY.Value + __instance.tilesHigh.Value) * 16 * 4) + offset) 
                : new Vector2(localX + 32.0f * 4.0f, localY + rectangle.Height * 4);
            
        var rotationCenter = new Vector2(ModEntry.AssetManager.PoleShadowTexture.Width / 2.0f,
            ModEntry.AssetManager.PoleShadowTexture.Height / 2.0f);
        // apply a 90deg rotation if placed sideways
        var shadowRotation = __instance.GetMetadata(Utility.Constants.MetadataIsPlacedSideways) == "true" ? (float)Math.PI / 2.0f : 0.0f;

        // alpha is a protected field, so we use a reflection
        //var alpha = localX == -1
        //    ? ModEntry.ModHelper.Reflection.GetField<float>(__instance, "alpha").GetValue()
        //    : 1f;
        
        b.Draw(ModEntry.AssetManager.PoleShadowTexture, position, null, Color.White, shadowRotation, rotationCenter, 4f, SpriteEffects.None, 1E-05f);

        return false; // don't run original method
    }
}