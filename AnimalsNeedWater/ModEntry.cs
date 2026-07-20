using AnimalsNeedWater.Core;
using AnimalsNeedWater.Core.Content;
using AnimalsNeedWater.Core.Content.Editors;
using AnimalsNeedWater.Core.Content.Loaders;
using AnimalsNeedWater.Core.Game;
using AnimalsNeedWater.Core.Models;
using AnimalsNeedWater.Core.Multiplayer;
using AnimalsNeedWater.Core.Patching;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;

namespace AnimalsNeedWater;

/// <summary> The mod entry class loaded by SMAPI. </summary>
public class ModEntry : Mod
{
    public static IMonitor ModMonitor = null!;
    public static IModHelper ModHelper = null!;
    public static ModConfig Config = null!;
    public static IManifest Manifest = null!;
    public static ModData Data = new ModData();

    public static PlacementRegistry PlacementRegistry = new PlacementRegistry();
    
    public static BuildingTracker BuildingTracker = new BuildingTracker();
    public static TroughManager TroughManager = new TroughManager();
    public static TroughVisuals TroughVisuals = new TroughVisuals();
    public static ThirstTracker ThirstTracker = new ThirstTracker();
    public static MessageBridge MessageBridge = new MessageBridge();

    /// <summary> The mod entry point, called after the mod is first loaded. </summary>
    /// <param name="helper"> Provides simplified APIs for writing mods. </param>
    public override void Entry(IModHelper helper)
    {
        ModHelper = helper;
        ModMonitor = Monitor;
        Manifest = ModManifest;
        
        Config = Helper.ReadConfig<ModConfig>();
        
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        helper.Events.GameLoop.Saving += OnSaving;
        helper.Events.GameLoop.DayEnding += OnDayEnding;
        helper.Events.GameLoop.DayStarted += OnDayStarted;
        
        helper.Events.World.BuildingListChanged += OnBuildingListChanged;
        
        helper.Events.Multiplayer.ModMessageReceived += MessageBridge.OnModMessageReceived;
        
        helper.Events.Content.AssetRequested += WaterBowlContentEditor.OnAssetRequested;
        helper.Events.Content.AssetRequested += DefaultAssetLoader.OnAssetRequested;
        
        helper.Events.Content.AssetRequested += PlacementRegistry.OnAssetRequested;
        helper.Events.Content.AssetsInvalidated += PlacementRegistry.OnAssetsInvalidated;
        helper.Events.Content.AssetsInvalidated += TroughVisuals.OnAssetsInvalidated;
    }

    public void SaveConfig(ModConfig newConfig)
    {
        Config = newConfig;
        Helper.WriteConfig(newConfig);
    }

    /// <summary> Get ANW's API </summary>
    /// <returns> API instance </returns>
    public override object GetApi()
    {
        return new API();
    }
    
    /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var harmony = new Harmony(ModManifest.UniqueID);
        
        ModMonitor.VerboseLog("Patching GameLocation.performToolAction.");
        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performToolAction)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.GameLocationToolAction))
        );
        
        ModMonitor.VerboseLog("Patching FarmAnimal.dayUpdate.");
        harmony.Patch(
            AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.dayUpdate)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.AnimalDayUpdate))
        );

        ModMonitor.VerboseLog("Patching FarmAnimal.behaviors.");
        harmony.Patch(
            AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.behaviors)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.AnimalBehaviors))
        );

        ModMonitor.VerboseLog("Patching Game1.OnLocationChanged.");
        harmony.Patch(
            AccessTools.Method(typeof(Game1), nameof(Game1.OnLocationChanged)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.OnLocationChanged))
        );
        
        ModMonitor.VerboseLog("Patching Building.draw.");
        harmony.Patch(
            AccessTools.Method(typeof(Building), nameof(Building.draw), new []{ typeof(SpriteBatch) }),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.BuildingDrawPostfix))
        );
        
        ModMonitor.VerboseLog("Done patching.");
        
        ModConfig.SetUpModConfigMenu(Config, this);
    }

    /// <summary>
    /// Read any saved data (thirsty animals/watered buildings) from the savefile. 
    /// </summary>
    private void LoadSavedModData()
    {
        if (!Context.IsMainPlayer)
        {
            Data = new ModData();
            return;
        }
        
        Data = Helper.Data.ReadSaveData<ModData>(ModConstants.ModDataSaveDataKey) ?? new ModData();
        Data.RestoreComparers();
    }

    /// <summary>
    /// Write data to the savefile.
    /// </summary>
    private void SaveModData()
    {
        if (!Context.IsMainPlayer) return;
        Helper.Data.WriteSaveData(ModConstants.ModDataSaveDataKey, Data);
    }
    
    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        if (Context.IsMainPlayer) return;
        
        // farmhand pulls state from host
        MessageBridge.SendRequestStateMessage(Game1.player.UniqueMultiplayerID);
    }

    /// <summary> Raised after the save is loaded. </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        LoadSavedModData();
        BuildingTracker.Refresh();

        if (Context.IsMainPlayer)
        {
            BuildingTracker.CheckHomeStatus();

            // if today's a festival, give the player a treat by filling all water troughs (just like with animal food in vanilla)
            if (!StardewValley.Utility.isFestivalDay(Game1.dayOfMonth, Game1.season))
            {
                TroughManager.EmptyWaterTroughs();
            }
            else
            {
                TroughManager.FillAllWaterTroughs();
            }
            
            Data.BuildingsWithBonusAwardedToday.Clear();
            ThirstTracker.ResetFullAnimals();
            
            MessageBridge.SendStateSyncMessage(Data.BuildingsWithWateredTrough, Data.BuildingsWithBonusAwardedToday, Data.FullAnimals);
            MessageBridge.SendThirstyAnimalsMessage(ThirstTracker.AnimalsLeftThirstyYesterday);
            
            // notify player of any animals left thirsty.
            // farmhands, if any, do this when receiving the ThirstyAnimalsMessage sent above
            if (Config.ShowAnimalsLeftThirstyMessage)
            {
                ThirstTracker.ShowLeftThirstyMessage();
            }
        }

        
        foreach (Building building in BuildingTracker.AnimalBuildings)
        {
            TroughVisuals.ReapplyVisuals(building);
        }
    }

    private void OnSaving(object? sender, SavingEventArgs e)
    {
        SaveModData();
    }
    
    private void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        if (!Context.IsMainPlayer) return; // only host computes the list; this is then broadcast to farmhands on day start
        ThirstTracker.FindThirstyAnimals();
    }
    
    public void OnBuildingListChanged(object? sender, BuildingListChangedEventArgs e)
    { 
        BuildingTracker.Refresh();
        
        foreach (var added in e.Added)
        {
            if (added.GetIndoors() is not AnimalHouse) continue;
            TroughVisuals.ReapplyVisuals(added);
        }

        foreach (var removed in e.Removed)
        {
            if (removed.GetIndoors() is not AnimalHouse) continue;
            TroughManager.ClearWatered(removed);
        }
    }
}
