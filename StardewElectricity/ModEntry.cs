using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewElectricity.Buildings;
using StardewElectricity.Managers;
using StardewElectricity.Types;
using StardewElectricity.Patching;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.Menus;
using StardewValley.Mods;
using Constants = StardewElectricity.Types.Constants;


namespace StardewElectricity
{
    /// <summary> The mod entry class loaded by SMAPI. </summary>
    public class ModEntry : Mod
    {
        public static ModEntry Instance;
        public static IMonitor ModMonitor;
        public ModConfig Config;
        
        public static Texture2D PoleTexture;
        public static Texture2D SidewaysPoleTexture;
        
        public static Texture2D PoleShadowTexture;

        public static ElectricityManager ElectricityManager;
        public static PoleManager PoleManager;

        /// <summary> The mod entry point, called after the mod is first loaded. </summary>
        /// <param name="helper"> Provides simplified APIs for writing mods. </param>
        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            Instance = this;

            helper.Events.Display.RenderingStep += OnRenderingStep;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.World.BuildingListChanged += OnBuildingListChanged;

            helper.Events.Content.AssetRequested += OnAssetRequested;

            ElectricityManager = new ElectricityManager();
            PoleManager = new PoleManager();

            Config = Helper.ReadConfig<ModConfig>();
            PrepareAssets();
        }

        public void SaveConfig(ModConfig newConfig)
        {
            Config = newConfig;
            Helper.WriteConfig(newConfig);
        }

        /// <summary>
        /// Prepare textures.
        /// </summary>
        private void PrepareAssets()
        {
            PoleTexture = Helper.ModContent.Load<Texture2D>("assets/utilityPole.png");
            SidewaysPoleTexture = Helper.ModContent.Load<Texture2D>("assets/utilityPoleSideways.png");
            PoleShadowTexture = Helper.ModContent.Load<Texture2D>("assets/utilityPole_shadow.png");
        }
        
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Buildings"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, BuildingData>().Data;
                    
                    data.Add(Constants.UtilityPoleBuildingTypeName,
                        new BuildingData()
                        {
                            Name = "Utility Pole",
                            Description = "Allows for electricity transmission within the farm bounds.",
                            Texture = Helper.ModContent.GetInternalAssetName(PoleTexture.Name).Name,
                            DrawShadow = true,
                            DrawOffset = new Vector2(-24.0f, -4.0f),
                            FadeWhenBehind = true,
                            Builder = "Robin",
                            BuildDays = 0,
                            BuildCost = 100,
                            BuildMaterials = new List<BuildingMaterial>()
                            {
                                new BuildingMaterial() { ItemId = "(O)388", Amount = 50 }
                            },
                            Skins = new List<BuildingSkin>()
                            {
                                new BuildingSkin()
                                {
                                    Id = $"{Helper.ModContent.ModID}_{Constants.SkinUtilityPoleSide}",
                                    Name = "Utility Pole",
                                    Description = "Allows for electricity transmission within the farm bounds.",
                                    Texture = Helper.ModContent.GetInternalAssetName(SidewaysPoleTexture.Name).Name,
                                    Metadata = new Dictionary<string, string> { { Constants.MetadataIsPlacedSideways, "true" } }
                                }
                            },
                            Metadata = new Dictionary<string, string> { { Constants.MetadataIsPlacedSideways, "false" } },
                        });
                });
            }
        }
        
        private void OnRenderingStep(object sender, RenderingStepEventArgs e)
        {
            if (e.Step != RenderSteps.World_Sorted)
                return;
            if (!Game1.player.currentLocation.IsFarm)
                return;
            PoleManager.Draw(e.SpriteBatch);
        }
        
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            PoleManager.SaveLoaded();
        }

        private void OnBuildingListChanged(object sender, BuildingListChangedEventArgs e)
        {
            if (e.Added.Any(b => b.buildingType.Value == Constants.UtilityPoleBuildingTypeName)
                || e.Removed.Any(b => b.buildingType.Value == Constants.UtilityPoleBuildingTypeName))
            {
                PoleManager.UpdatePoles();
                PoleManager.DoWiring(PoleManager.PolesOnFarm);
            }
        }

        private void ApplyHarmonyPatches()
        {
            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                AccessTools.Method(typeof(Building), nameof(Building.drawShadow)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.BuildingDrawShadow))
            );
            harmony.Patch(
                AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.draw), new []{ typeof(SpriteBatch) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.CarpenterMenuDraw))
            );
        }

        /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ApplyHarmonyPatches();
            ModConfig.SetUpModConfigMenu(Config, this);
        }
    }
}