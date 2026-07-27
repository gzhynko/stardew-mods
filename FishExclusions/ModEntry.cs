using System;
using System.Linq;
using System.Reflection;
using FishExclusions.Core;
using FishExclusions.Core.Integrations.Config;
using FishExclusions.Core.Models;
using FishExclusions.Core.Patching;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace FishExclusions;

/// <summary> The mod entry class loaded by SMAPI. </summary>
public class ModEntry : Mod
{
    public static IModHelper ModHelper = null!;
    public static IMonitor ModMonitor = null!;
    public static ModConfig Config = null!;

    public static bool ExclusionsEnabled = true;

    /// <summary> The mod entry point, called after the mod is first loaded. </summary>
    /// <param name="helper"> Provides simplified APIs for writing mods. </param>
    public override void Entry(IModHelper helper)
    {
        ModHelper = Helper;
        ModMonitor = Monitor;

        try
        {
            Config = Helper.ReadConfig<ModConfig>();
        }
        catch (Exception exception)
        {
            // Notify user of invalid config.
            ModMonitor.Log(
                $"Config file is formatted incorrectly, mod will not work correctly. Details: {exception.Message}",
                LogLevel.Warn);
        }

        Config ??= new ModConfig();

        CommandManager.RegisterCommands(helper);

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += OnFirstSaveLoaded;
    }

    public void SaveConfig(ModConfig newConfig)
    {
        Config = newConfig;
        Helper.WriteConfig(newConfig);
    }

    public static void ReloadConfig()
    {
        try
        {
            Config = ModHelper.ReadConfig<ModConfig>();
        }
        catch (Exception exception)
        {
            ModMonitor.Log($"Failed to reload config, keeping the previous one. Details: {exception.Message}",
                LogLevel.Warn);
        }
    }

    private static readonly MethodBase[] PatchedMethods =
    [
        AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
        AccessTools.Method(typeof(MineShaft), nameof(MineShaft.getFish))
    ];

    private void ApplyHarmonyPatches()
    {
        var harmony = new Harmony(ModManifest.UniqueID);

        foreach (var method in PatchedMethods)
        {
            harmony.Patch(method, postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.GetFish)));
        }
    }

    private void OnFirstSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        Helper.Events.GameLoop.SaveLoaded -= OnFirstSaveLoaded;
        LogPatchConflicts();
    }

    /// <summary>
    /// mods affecting fishing are common; this helps debugging
    /// </summary>
    private void LogPatchConflicts()
    {
        foreach (var method in PatchedMethods)
        {
            var patchInfo = Harmony.GetPatchInfo(method);
            if (patchInfo == null) continue;

            var otherOwners = patchInfo.Prefixes
                .Concat(patchInfo.Postfixes)
                .Concat(patchInfo.Transpilers)
                .Concat(patchInfo.Finalizers)
                .Select(patch => patch.owner)
                .Where(owner => owner != ModManifest.UniqueID)
                .Distinct()
                .ToList();

            if (otherOwners.Count == 0) continue;

            var ownerNames = otherOwners
                .Select(owner => Helper.ModRegistry.Get(owner)?.Manifest.Name ?? owner);
            Monitor.Log(
                $"Another mod also patches {method.DeclaringType?.Name}.{method.Name}: {string.Join(", ", ownerNames)}. " +
                "This may cause conflicts with this mod's functionality.",
                LogLevel.Warn);
        }
    }

    /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        ApplyHarmonyPatches();
        ConfigMenu.Register(Config, this);
    }
}