using HarmonyLib;
using ScytheHarvestsMore.Models;
using ScytheHarvestsMore.Patching;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace ScytheHarvestsMore;

public class ModEntry : Mod
{
    public static IModHelper ModHelper = null!;
    public static IMonitor ModMonitor = null!;
    public static ModConfig Config = null!;

    public override void Entry(IModHelper helper)
    {
        ModHelper = Helper;
        ModMonitor = Monitor;
        
        Config = Helper.ReadConfig<ModConfig>();
        
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }
    
    public void SaveConfig(ModConfig newConfig)
    {
        Config = newConfig;
        Helper.WriteConfig(newConfig);
    }
    
    private void ApplyHarmonyPatches()
    {
        var harmony = new Harmony(ModManifest.UniqueID);

        harmony.Patch(
            AccessTools.Method(typeof(FruitTree), nameof(FruitTree.performToolAction)),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.FruitTreePerformToolAction))
        );
        harmony.Patch(
            AccessTools.Method(typeof(Object), nameof(Object.performToolAction)),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.ObjectPerformToolAction))
        );
    }

    /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        ModConfig.SetUpModConfigMenu(Config, this);

        ApplyHarmonyPatches();
    }
}