using StardewModdingAPI;

namespace ScytheHarvestsMore.Models
{
    public interface IGenericModConfigMenuApi
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);
        void AddSectionTitle(IManifest mod, Func<string> text, Func<string> tooltip = null!);
        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null!, string fieldId = null!);
        void AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name, Func<string> tooltip = null!, float? min = null, float? max = null, float? interval = null, Func<float, string> formatValue = null!, string fieldId = null!);
    }
    
    /// <summary> The mod config class. More info here: https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Config </summary>
    public class ModConfig
    {
        /// <summary>
        /// Whether the iridium scythe harvests machines.
        /// </summary>
        public bool HarvestMachines { get; set; } = true;
        
        /// <summary>
        /// Whether the iridium scythe harvests fruit trees.
        /// </summary>
        public bool HarvestFruitTrees { get; set; } = true;
        
        /// <summary>
        /// Whether the iridium scythe harvests forage items.
        /// </summary>
        public bool HarvestForage { get; set; } = true;

        /// <summary>
        /// Setup the Generic Mod Config Menu API.
        /// </summary>
        public static void SetUpModConfigMenu(ModConfig config, ModEntry mod)
        {
            IGenericModConfigMenuApi? api = mod.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (api == null) return;

            var manifest = mod.ModManifest;

            api.Register(manifest, () =>
            {
                config = new ModConfig();
                mod.SaveConfig(config);
            }, () => mod.SaveConfig(config));
            
            api.AddSectionTitle(manifest, () => "Functionality");
            
            api.AddBoolOption(manifest, () => config.HarvestMachines, val => config.HarvestMachines = val,
                () => "Harvest Machines",
                () => "Whether the iridium scythe harvests machines.");
            api.AddBoolOption(manifest, () => config.HarvestFruitTrees, val => config.HarvestFruitTrees = val,
                () => "Harvest Fruit Trees", 
                () => "Whether the iridium scythe harvests fruit trees.");
            api.AddBoolOption(manifest, () => config.HarvestFruitTrees, val => config.HarvestFruitTrees = val,
                () => "Harvest Forage", 
                () => "Whether the iridium scythe harvests forage items.");

        }
    }
}