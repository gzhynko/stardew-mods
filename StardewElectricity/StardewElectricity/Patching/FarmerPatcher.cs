using Common.Patching;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
// ReSharper disable InconsistentNaming

namespace StardewElectricity.Patching;

public class FarmerPatcher : BasePatcher
{
    public override void Apply(Harmony harmony, IMonitor modMonitor)
    {
        harmony.Patch(
            AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new []{ typeof(SpriteBatch) }),
            prefix: new HarmonyMethod(this.GetType(), nameof(FarmerPatcher.Draw_Prefix))
        );
    }

    private static void Draw_Prefix(Farmer __instance, SpriteBatch b)
    {
        if (Game1.activeClickableMenu != null) return;
        
        var heldObject = __instance.ActiveObject;
        if (heldObject != null && Utility.Utility.IsConsumer(heldObject.QualifiedItemId))
        {
            ModEntry.PoleManager.DrawPoleCoverage(b);
        }
    } 
}