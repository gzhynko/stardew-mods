using System;
using System.Collections.Generic;
using System.Linq;
using CropGrowthAdjustments.Core.Content;
using CropGrowthAdjustments.Core.Game;
using CropGrowthAdjustments.Core.Integrations;
using CropGrowthAdjustments.Core.Patching;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.TerrainFeatures;

namespace CropGrowthAdjustments
{
    /// <summary> The mod entry class loaded by SMAPI. </summary>
    public class ModEntry : Mod
    {
        public static IMonitor ModMonitor = null!;
        public static IModHelper ModHelper = null!;
        
        public static ContentPackLoader ContentPackLoader = new();
        public static AdjustmentResolver AdjustmentResolver = new();
        public static SeasonalTextureFactory SeasonalTextures = new();
        public static CropSpriteManager CropSpriteManager = new();

        /// <summary> The mod entry point, called after the mod is first loaded. </summary>
        /// <param name="helper"> Provides simplified APIs for writing mods. </param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Content.AssetRequested += OnAssetRequested;
            
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += CropSpriteManager.OnDayStarted;
            
            helper.Events.Player.Warped += CropSpriteManager.OnWarped;

            ModMonitor = Monitor;
            ModHelper = helper;
            
            ContentPackLoader.LoadContentPacks(helper, Monitor);
        }
        
        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Crops"))
            {
                e.Edit(EditCropData, AssetEditPriority.Late);
            }
            else
            {
                SeasonalTextures.TryHandleAssetRequest(e);
            }
        }
        
        private void EditCropData(IAssetData asset)
        {
            var cropData = asset.AsDictionary<string, CropData>();
            
            foreach (var adjustments in ContentPackLoader.ContentPacks)
            {
                foreach (var cropAdjustment in adjustments.CropAdjustments)
                {
                    // not resolved yet; the resolver invalidates this asset once ids are known
                    if (cropAdjustment.CropProduceItemId == "-1") continue;

                    var produceId = Core.Utility.NormalizeItemId(cropAdjustment.CropProduceItemId);
                            
                    foreach (var entry in cropData.Data.Values)
                    {
                        if (entry.HarvestItemId == null || Core.Utility.NormalizeItemId(entry.HarvestItemId) != produceId) continue;

                        // union produce, grow, and hibernate seasons here - the game logic then keeps the crop alive in all of them
                        var seasons = new List<Season>(cropAdjustment.ParsedSeasonsToGrowIn);
                        foreach (var season in cropAdjustment.ParsedSeasonsToProduceIn.Concat(cropAdjustment.ParsedSeasonsToHibernateIn))
                        {
                            if (!seasons.Contains(season)) seasons.Add(season);
                        }
                        entry.Seasons = seasons;
                        
                        break;
                    }
                }
            }
        }
        
        private void ApplyHarmonyPatches()
        {
            var harmony = new Harmony("GZhynko.CropGrowthAdjustments");
            
            harmony.Patch(
                AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.plant)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.HoeDirtPlant))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Crop), nameof(Crop.newDay)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.CropNewDay))
            );
        }

        /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            ApplyHarmonyPatches();

            var jsonAssetsApi = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (jsonAssetsApi != null)
            {
                jsonAssetsApi.ItemsRegistered += OnJAItemsRegistered;
            }
        }
        
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            ResolveAdjustments();
        }

        private void ResolveAdjustments()
        {
            AdjustmentResolver.ResolveAll(Helper);
            SeasonalTextures.RegisterAssets(Helper);
        }
        
        private void OnJAItemsRegistered(object? sender, EventArgs eventArgs)
        {
            if (Context.IsWorldReady) ResolveAdjustments();
        }
    }
}
