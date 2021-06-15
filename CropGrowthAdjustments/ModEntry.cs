using System;
using System.Collections.Generic;
using CropGrowthAdjustments.Types;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace CropGrowthAdjustments
{
    /// <summary> The mod entry class loaded by SMAPI. </summary>
    public class ModEntry : Mod, IAssetEditor
    {
        public static ContentPackManager ContentPackManager = new ContentPackManager();
        public static IMonitor ModMonitor;

        private IJsonAssetsApi _jsonAssetsApi;
        
        #region Public methods

        /// <summary> The mod entry point, called after the mod is first loaded. </summary>
        /// <param name="helper"> Provides simplified APIs for writing mods. </param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            ContentPackManager.InitializeContentPacks(helper, Monitor);

            ModMonitor = Monitor;
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data/Crops") || asset.AssetNameEquals("TileSheets/crops");
        }

        /// <summary>Edit crop data to adjust the growth seasons and the crop spritesheet to add special sprites.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            if (!asset.AssetNameEquals("Data/Crops") && !asset.AssetNameEquals("TileSheets/crops")) return;

            if (asset.AssetNameEquals("TileSheets/crops"))
            {
                var editor = asset.AsImage();

                var totalNewSpritesCount = 0;
                
                foreach (var adjustments in ContentPackManager.ContentPacks)
                {
                    foreach (var cropAdjustment in adjustments.CropAdjustments)
                    {
                        if(cropAdjustment.SpecialSpritesForSeasons.Count == 0) continue;

                        foreach (var _ in cropAdjustment.SpecialSpritesForSeasons)
                        {
                            totalNewSpritesCount++;
                        }
                    }
                }

                int minHeight = (int) (768 + Math.Ceiling((decimal) (totalNewSpritesCount / 2f)) * 32);

                // Expand the spritesheet to the bottom to fit the special sprites.
                editor.ExtendImage(0, minHeight);

                // 48 is the first row unused by the game.
                var currentRowInSpriteSheet = 48;
                
                foreach (var adjustments in ContentPackManager.ContentPacks)
                {
                    foreach (var cropAdjustment in adjustments.CropAdjustments)
                    {
                        if(cropAdjustment.SpecialSpritesForSeasons.Count == 0) continue;

                        foreach (var specialSprite in cropAdjustment.SpecialSpritesForSeasons)
                        {
                            // Abort adding special sprites if JsonAssets has custom crops loaded and the limit of 100 rows is reached.
                            if (Utility.JsonAssetsHasCropsLoaded(_jsonAssetsApi) && currentRowInSpriteSheet >= 99)
                            {
                                Monitor.Log($"Special sprites for {adjustments.ContentPack.Manifest.Name} ({specialSprite.Season}) cannot be fully loaded, since the limit of 100 rows (set by JsonAssets) in the crops tilesheet is reached. The content pack will continue to work correctly, though you won't see the special sprites.", LogLevel.Warn);
                                return;
                            }
                            
                            Texture2D sourceImage;
                            
                            try
                            {
                                sourceImage = adjustments.ContentPack.LoadAsset<Texture2D>(specialSprite.Sprites);
                            }
                            catch (Exception)
                            {
                                Monitor.Log($"[{adjustments.ContentPack.Manifest.Name}] - Couldn't load special sprites for {specialSprite.Season}. They will not be shown.", LogLevel.Error);
                                continue;
                            }

                            int xCoordinate = currentRowInSpriteSheet % 2 == 0 ? 0 : 128;
                            int yCoordinate = (int) (Math.Floor(currentRowInSpriteSheet / 2f) * 32);
                            
                            editor.PatchImage(sourceImage, targetArea: new Rectangle(xCoordinate, yCoordinate, 128, 32));

                            specialSprite.RowInSpriteSheet = currentRowInSpriteSheet;
                            currentRowInSpriteSheet++;
                        }
                    }
                }

                return;
            }
            
            IDictionary<int, string> cropData = asset.AsDictionary<int, string>().Data;
            
            foreach (var adjustments in ContentPackManager.ContentPacks)
            {
                foreach (var cropAdjustment in adjustments.CropAdjustments)
                {
                    var cropProduceItemId = cropAdjustment.CropProduceItemId;
                    if (cropProduceItemId == -1) continue;
                    
                    foreach (var itemId in cropData.Keys)
                    {
                        var itemData = cropData[itemId];
                        var fields = itemData.Split('/');
                        
                        if(int.Parse(fields[3]) != cropProduceItemId) continue;
                        
                        fields[1] = cropAdjustment.GetSeasonsToGrowIn().Join(delimiter: " ");
                        cropData[itemId] = string.Join("/", fields);
                        
                        break;
                    }
                }
            }
        }
        
        #endregion
        #region Private methods
        
        private void ApplyHarmonyPatches()
        {
            var harmony = HarmonyInstance.Create("GZhynko.CropGrowthAdjustments");
            
            harmony.Patch(
                AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.dayUpdate)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.HoeDirtDayUpdate))
            );

            harmony.Patch(
                AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.draw)), 
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.HoeDirtDraw))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Crop), nameof(Crop.newDay)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.CropNewDay))
            );
        }

        /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ApplyHarmonyPatches();
            
            var jsonAssetsApi = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            _jsonAssetsApi = jsonAssetsApi;

            if (jsonAssetsApi != null)
            {
                jsonAssetsApi.IdsAssigned += (o, args) => OnJsonAssetsIdsAssigned(jsonAssetsApi);
            }
            else
            {
                ContentPackManager.AssignCropProduceItemIds(Helper, null);
                ContentPackManager.AssignCropOriginalRowsInSpritesheet(Helper);
            }
        }

        private void OnJsonAssetsIdsAssigned(IJsonAssetsApi jsonAssetsApi)
        {
            ContentPackManager.AssignCropProduceItemIds(Helper, jsonAssetsApi);
            Helper.Content.InvalidateCache("Data/Crops");
            
            ContentPackManager.AssignCropOriginalRowsInSpritesheet(Helper);
        }

        #endregion
    }
}
