using System;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace DialogueBoxRedesign.Patching;

internal class HarmonyPatches
{
    public static bool DrawPortrait(DialogueBox __instance, SpriteBatch b)
    {
        try
        {
            return HarmonyPatchExecutors.DrawPortrait(__instance, b);
            ;
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in {nameof(DrawPortrait)}:\n{e}", LogLevel.Error);

            return true;
        }
    }

    public static bool DrawBox(DialogueBox __instance, SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
    {
        try
        {
            return HarmonyPatchExecutors.DrawBox(__instance, b, xPos, yPos, boxWidth, boxHeight);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in {nameof(DrawBox)}:\n{e}", LogLevel.Error);

            return true;
        }
    }

    public static bool Draw(DialogueBox __instance, SpriteBatch b)
    {
        try
        {
            return HarmonyPatchExecutors.Draw(__instance, b);
        }
        catch (Exception e)
        {
            ModEntry.ModMonitor.Log($"Failed in {nameof(Draw)}:\n{e}", LogLevel.Error);
            return true;
        }
    }
}