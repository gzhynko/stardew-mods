using System;
using System.Collections.Generic;
using Common.Patching;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using Constants = StardewElectricity.Utility.Constants;
using Object = StardewValley.Object;

namespace StardewElectricity.Patching;

public class ObjectPatcher : BasePatcher
{
    public override void Apply(Harmony harmony, IMonitor modMonitor)
    {
        harmony.Patch(
            AccessTools.Method(typeof(Object), nameof(Object.minutesElapsed)),
            prefix: new HarmonyMethod(this.GetType(), nameof(ObjectPatcher.MinutesElapsed_Prefix))
        );
        harmony.Patch(
            AccessTools.Method(typeof(Object), nameof(Object.ShouldTimePassForMachine)),
            prefix: new HarmonyMethod(this.GetType(), nameof(ObjectPatcher.ShouldTimePassForMachine_Prefix))
        );
        harmony.Patch(
            AccessTools.Method(typeof(Object), nameof(Object.ShouldWobble)),
            prefix: new HarmonyMethod(this.GetType(), nameof(ObjectPatcher.ShouldWobble_Prefix))
        );
        harmony.Patch(
            AccessTools.Method(typeof(Object), nameof(Object.draw), new []{ typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
            postfix: new HarmonyMethod(this.GetType(), nameof(ObjectPatcher.Draw_Postfix))
        );
    }

    private static bool MinutesElapsed_Prefix(Object __instance, int minutes)
    {
        GameLocation location = __instance.Location;
        if (location == null)
            return false;
        if (__instance.heldObject.Value != null && Utility.Utility.IsConsumer(__instance.QualifiedItemId))
        {
            if (!ModEntry.PoleManager.IsTilePowered(__instance.Location, __instance.TileLocation))
                return false;

            ModEntry.ModMonitor.Log($"{__instance.name} ({__instance.DisplayName}): minutes elapsed: {minutes}; remaining: {__instance.MinutesUntilReady}", LogLevel.Alert);

            if (__instance.MinutesUntilReady <= 0) return true;
            var consumed = Utility.Utility.GetKwhConsumedPer10Minutes(__instance.QualifiedItemId);
            if (consumed != null)
                ModEntry.ElectricityManager.ConsumeKwh(minutes / 10.0f * consumed.Value);
        }

        return true;
    }
    
    private static bool ShouldTimePassForMachine_Prefix(Object __instance, ref bool __result)
    {
        if (__instance.Location != null 
            && Utility.Utility.IsConsumer(__instance.QualifiedItemId) 
            && !ModEntry.PoleManager.IsTilePowered(__instance.Location, __instance.TileLocation))
        {
            __result = false;
            return false;
        }
        return true;
    }

    private static bool ShouldWobble_Prefix(Object __instance, ref bool __result)
    {
        if (__instance.Location != null 
            && Utility.Utility.IsConsumer(__instance.QualifiedItemId) 
            && !ModEntry.PoleManager.IsTilePowered(__instance.Location, __instance.TileLocation))
        {
            __result = false;
            return false;
        }
        return true;
    }

    private static void Draw_Postfix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
    {
        if (__instance.Location == null || __instance.readyForHarvest.Value || !Utility.Utility.IsConsumer(__instance.QualifiedItemId)) return;

        if (ModEntry.PoleManager.IsTilePowered(__instance.Location, __instance.TileLocation))
            return;
        float num1 = (float) ((double) ((y + 1) * 64) / 10000.0 + (double) __instance.TileLocation.X / 50000.0);
        if (__instance.IsTapper() || __instance.QualifiedItemId.Equals("(BC)MushroomLog"))
            num1 += 0.02f;
        float num2 = (float) (4.0 * Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2));
        spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float) (x * 64 - 8), (float) (y * 64 - 96 - 16) + num2)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, num1 + 1E-06f);
        spriteBatch.Draw(ModEntry.IconsTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float) (x * 64 + 32), (float) (y * 64 - 64 - 8) + num2)), new Rectangle(32, 0, 16, 16), Color.White * 0.75f, 0.0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, num1 + 1E-05f);
    }
}