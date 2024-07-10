using HarmonyLib;
using StardewModdingAPI;

namespace Common.Patching;

public interface IPatcher
{
    public void Apply(Harmony harmony, IMonitor modMonitor);
}