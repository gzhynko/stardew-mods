using HarmonyLib;
using StardewModdingAPI;

namespace Common.Patching;

public abstract class BasePatcher : IPatcher
{
    public abstract void Apply(Harmony harmony, IMonitor modMonitor);
}