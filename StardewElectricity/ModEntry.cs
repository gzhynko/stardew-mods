using System.Collections.Generic;
using System.Xml.Serialization;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewElectricity.Buildings;
using StardewElectricity.Editors;
using StardewElectricity.Loaders;
using StardewElectricity.Managers;
using StardewElectricity.Types;
using StardewElectricity.Patching;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;


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

            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;

            helper.Content.AssetEditors.Add(new DataEditor());
            helper.Content.AssetLoaders.Add(new AssetLoader());

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
            PoleTexture = Helper.Content.Load<Texture2D>("assets/utilityPole.png");
            SidewaysPoleTexture = Helper.Content.Load<Texture2D>("assets/utilityPoleSideways.png");
            PoleShadowTexture = Helper.Content.Load<Texture2D>("assets/utilityPole_shadow.png");
        }

        /// <summary>
        /// Draw the bars.
        /// </summary>
        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            PoleManager.Draw(e.SpriteBatch);
        }
        
        
        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is CarpenterMenu carpenterMenu)
            {
                var blueprints = Helper.Reflection.GetField<List<BluePrint>>(carpenterMenu, "blueprints").GetValue();
                
                blueprints.Add(new BluePrint(UtilityPole.BlueprintName));
            }
        }
        
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            PoleManager.SaveLoaded();
        }

        private void ApplyHarmonyPatches()
        {
            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            harmony.Patch(
                AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.setNewActiveBlueprint)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.SetNewActiveBlueprint))
            );
            
            harmony.Patch(
                AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.tryToBuild)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.TryToBuild))
            );
        }

        /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ApplyHarmonyPatches();
            ModConfig.SetUpModConfigMenu(Config, this);
            
            AddTypesToSerialize(Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore"));
        }

        private void AddTypesToSerialize(ISpaceCoreApi spaceCoreApi)
        {
            if (spaceCoreApi == null)
            {
                Monitor.Log("No SpaceCore API found. The game won't save properly. Please install SpaceCore to continue using this mod.", LogLevel.Error);
                return;
            }
            
            spaceCoreApi.RegisterSerializerType(typeof(UtilityPole));
        }
    }
}