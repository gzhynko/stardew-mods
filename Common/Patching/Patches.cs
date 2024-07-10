using System;
using HarmonyLib;
using StardewModdingAPI;

namespace Common.Patching;

public class Patches
{
    public static Harmony Apply(Mod mod, params IPatcher[] patchers)
    {
        Harmony harmony = new Harmony(mod.ModManifest.UniqueID);

        foreach (IPatcher patcher in patchers)
        {
            try
            {
                patcher.Apply(harmony, mod.Monitor);
            }
            catch (Exception e)
            {
                mod.Monitor.Log($"Unable to apply the '{patcher.GetType().FullName}' patcher. Details:\n{e}", LogLevel.Error);
            }
        }

        return harmony;
    }
}