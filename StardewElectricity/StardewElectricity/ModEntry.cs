using Microsoft.Xna.Framework;
using StardewElectricity.Managers;
using StardewElectricity.Types;
using StardewElectricity.Patching;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.Menus;
using StardewValley.Mods;
using Constants = StardewElectricity.Utility.Constants;
using Patches = Common.Patching.Patches;


namespace StardewElectricity
{
    /// <summary> The mod entry class loaded by SMAPI. </summary>
    public class ModEntry : Mod
    {
        public static IMonitor ModMonitor;
        public static IModHelper ModHelper;
        private ModConfig _config;

        public static AssetManager AssetManager;
        public static ElectricityManager ElectricityManager;
        public static PoleManager PoleManager;

        /// <summary> The mod entry point, called after the mod is first loaded. </summary>
        /// <param name="helper"> Provides simplified APIs for writing mods. </param>
        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            ModHelper = Helper;

            helper.Events.Display.RenderingStep += OnRenderingStep;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.World.BuildingListChanged += OnBuildingListChanged;
            helper.Events.GameLoop.DayEnding += OnDayEnding;

            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Display.MenuChanged += OnMenuChanged;

            AssetManager = new AssetManager();
            ElectricityManager = new ElectricityManager();
            PoleManager = new PoleManager();
            
            _config = Helper.ReadConfig<ModConfig>();
            AssetManager.PrepareAssets(Helper.ModContent);
        }

        public void SaveConfig(ModConfig newConfig)
        {
            _config = newConfig;
            Helper.WriteConfig(newConfig);
        }
        
        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Buildings"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, BuildingData>().Data;
                    
                    data.Add(Constants.UtilityPoleBuildingTypeName,
                        new BuildingData
                        {
                            Name = "Utility Pole",
                            Description = "Allows for electricity transmission within the farm bounds.",
                            Texture = Helper.ModContent.GetInternalAssetName(AssetManager.PoleTexture.Name).Name,
                            DrawShadow = true,
                            DrawOffset = new Vector2(-24.0f, -4.0f),
                            FadeWhenBehind = true,
                            Builder = "Robin",
                            BuildDays = 0,
                            BuildCost = 100,
                            BuildMaterials = new List<BuildingMaterial>()
                            {
                                new () { ItemId = "(O)388", Amount = 50 }
                            },
                            Skins = new List<BuildingSkin>()
                            {
                                new ()
                                {
                                    Id = Constants.SkinUtilityPoleSide,
                                    Name = "Utility Pole",
                                    Description = "Allows for electricity transmission within the farm bounds.",
                                    Texture = Helper.ModContent.GetInternalAssetName(AssetManager.SidewaysPoleTexture.Name).Name,
                                    Metadata = new Dictionary<string, string> { { Constants.MetadataIsPlacedSideways, "true" } }
                                }
                            },
                            Metadata = new Dictionary<string, string> { { Constants.MetadataIsPlacedSideways, "false" } },
                        });
                });
            }
        }
        
        private void OnRenderingStep(object? sender, RenderingStepEventArgs e)
        {
            if (e.Step == RenderSteps.World_Sorted)
            {
                if (!Game1.player.currentLocation.IsFarm || !Game1.player.currentLocation.IsOutdoors)
                    return;
                PoleManager.DrawWiring(e.SpriteBatch);
            }
        }
        
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            PoleManager.SaveLoaded();
            ElectricityManager.SaveLoaded();
        }

        private void OnBuildingListChanged(object? sender, BuildingListChangedEventArgs e)
        {
            if (e.Added.Any(b => b.buildingType.Value == Constants.UtilityPoleBuildingTypeName)
                || e.Removed.Any(b => b.buildingType.Value == Constants.UtilityPoleBuildingTypeName))
            {
                PoleManager.UpdateAll();
            }
            PoleManager.UpdateFarmBuildingTiles();
        }

        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is GameMenu menu)
            {
                Menus.Electricity.GameMenuOpened(menu);
            }
        }

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            ElectricityManager.DayEnding();
        }

        /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            ModConfig.SetUpModConfigMenu(_config, this);
            
            Patches.Apply(this,
                new BuildingPatcher(),
                new CarpenterMenuPatcher(),
                new GameMenuPatcher(),
                new FarmerPatcher(),
                new ObjectPatcher());
        }
    }
}