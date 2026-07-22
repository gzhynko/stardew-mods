using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace EventBlackBars;

public class HarmonyPatches
{
    public static void DrawAfterMap(SpriteBatch b)
    {
        if (!ModEntry.RenderBars) return;

        try
        {
            ModEntry.DrawBars(b);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in {nameof(DrawAfterMap)}:\n{e}", LogLevel.Error);
        }
    }

    /// <summary> Patch for the GameLocation.startEvent method. </summary>
    public static void EventStart(Event evt)
    {
        if (evt.isFestival || evt.isWedding) return;

        try
        {
            ModEntry.Instance.StartMovingBars(Direction.MoveIn);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in {nameof(EventStart)}:\n{e}", LogLevel.Error);
        }
    }
}