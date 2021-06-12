using System;
using StardewModdingAPI;

namespace EventBlackBars
{
    public interface IGenericModConfigMenuApi
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);

        void SetDefaultIngameOptinValue( IManifest mod, bool optedIn );
        
        void RegisterLabel(IManifest mod, string labelName, string labelDesc);
        void RegisterParagraph(IManifest mod, string paragraph);
        
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet);

        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max);
    }
    
    /// <summary> The mod config class. More info here: https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Config </summary>
    public class ModConfig
    {
        /// <summary>
        /// The percentage of the height of the screen for a bar to take up.
        /// </summary>
        public double BarHeightPercentage { get; set; } = 10;
        
        /// <summary>
        /// Whether to gradually move the bars in when an event starts, or have them fully out right away.
        /// </summary>
        public bool MoveBarsInSmoothly { get; set; } = true;
        
        /// <summary>
        /// Setup the Generic Mod Config Menu API.
        /// </summary>
        public static void SetUpModConfigMenu(ModConfig config, ModEntry mod)
        {
            IGenericModConfigMenuApi api = mod.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (api == null) return;

            var manifest = mod.ModManifest;

            api.RegisterModConfig(manifest, () => config = new ModConfig(), delegate { mod.Helper.WriteConfig(config); });
            api.SetDefaultIngameOptinValue(manifest, true);

            api.RegisterLabel(manifest, "Appearance", null);

            api.RegisterClampedOption(manifest, "Bar Height Percentage", "The percentage of the height of the screen for a bar to take up.", () => (float)config.BarHeightPercentage, (float val) => config.BarHeightPercentage = val, 1, 49);
            api.RegisterSimpleOption(manifest, "Move Bars In Smoothly", "Whether to gradually move the bars in when an event starts, or have them fully out right away.", () => config.MoveBarsInSmoothly, (bool val) => config.MoveBarsInSmoothly = val);
        }
    }
}
