using DialogueBoxRedesign.Patching;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace DialogueBoxRedesign
{
    /// <summary> The mod entry class loaded by SMAPI. </summary>
    public class ModEntry : Mod
    {
        #region Variables
        
        public static IMonitor ModMonitor;
        public static IModHelper ModHelper;
        public static ModConfig Config;

        public static Texture2D GradientSample;
        public static Texture2D DarkerGradientSample;

        #endregion
        #region Public methods
        
        /// <summary> The mod entry point, called after the mod is first loaded. </summary>
        /// <param name="helper"> Provides simplified APIs for writing mods. </param>
        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            ModHelper = Helper;
            
            Config = Helper.ReadConfig<ModConfig>();
            
            PrepareAssets();
            
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        #endregion
        #region Private methods
        
        private void PrepareAssets()
        {
            GradientSample = Helper.Content.Load<Texture2D>("assets/gradientSample.png");
            DarkerGradientSample = Helper.Content.Load<Texture2D>("assets/darkerGradientSample.png");
        }
        
        private void ApplyHarmonyPatches()
        {
            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            harmony.Patch(
                AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.drawPortrait)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.DrawPortrait))
            );
            
            harmony.Patch(
                AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.drawBox)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.DrawBox))
            );
            
            harmony.Patch(
                AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.draw), new [] { typeof(SpriteBatch) }),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.Draw))
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
