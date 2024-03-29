﻿using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace GrapesAllYearRound
{
    /// <summary> The mod entry class loaded by SMAPI. </summary>
    public class ModEntry : Mod
    {
        #region Public methods

        /// <summary> The mod entry point, called after the mod is first loaded. </summary>
        /// <param name="helper"> Provides simplified APIs for writing mods. </param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("TileSheets/crops"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsImage();
                    Texture2D sourceImage;

                    try
                    {
                        sourceImage = Helper.ModContent.Load<Texture2D>("assets/grape_winter.png");
                    }
                    catch (Microsoft.Xna.Framework.Content.ContentLoadException)
                    {
                        Monitor.Log("Couldn't load the winter sprites. The mod will work incorrectly.", LogLevel.Error);
                        return;
                    }

                    // Expand the spritesheet to the bottom to fit the winter sprites. They should be on row 48.
                    editor.ExtendImage(0, 800);
                    editor.PatchImage(sourceImage, targetArea: new Rectangle(0, 768, 128, 32));
                });
            }
            else if(e.Name.IsEquivalentTo("Data/Crops"))
            {
                e.Edit(asset =>
                {
                    IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;
                    foreach (var itemId in data.Keys)
                    {
                        if (itemId != 301) continue;
                    
                        var fields = data[itemId].Split('/');
                        fields[1] = "spring summer fall winter";
                        data[itemId] = string.Join("/", fields);
                
                        break;
                    }
                });
            }
        }
        
        #endregion
        #region Private methods
        
        private void ApplyHarmonyPatches()
        {
            var harmony = new Harmony(ModManifest.UniqueID);
            
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
        }
        
        #endregion
    }
}
