using System;
using StardewModdingAPI;

namespace AnimalsNeedWater.Core.Models
{
    public interface IGenericModConfigMenuApi
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);
        void AddSectionTitle(IManifest mod, Func<string> text, Func<string>? tooltip = null);
        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);
        void AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name, Func<string>? tooltip = null, float? min = null, float? max = null, float? interval = null, Func<float, string>? formatValue = null, string? fieldId = null);
    }
    
    /// <summary> The mod config class. More info here: https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Config </summary>
    public class ModConfig
    {
        /// <summary>
        /// Whether to show "love" bubbles over animals inside the building when watered the trough.
        /// </summary>
        public bool ShowLoveBubblesOverAnimalsWhenWateredTrough { get; set; } = true;

        /// <summary>
        /// Whether to show a message (bottom left corner of screen) in the morning telling how many animals (if any)
        /// were left thirsty last night. 
        /// </summary>
        public bool ShowAnimalsLeftThirstyMessage { get; set; } = true;
        
        [Obsolete("Use UseWateringSystems instead")]
        public bool? WateringSystemInDeluxeBuildings { get; set; }
        public bool ShouldSerializeWateringSystemInDeluxeBuildings() => false;

        /// <summary>
        /// Whether to enable watering systems in certain buildings (e.g. Deluxe Barn, Deluxe Coop).
        /// </summary>
        public bool UseWateringSystems { get; set; } = true;

        /// <summary>
        /// Whether to replace coop's and big coop's textures when troughs inside them are empty.
        /// </summary>
        public bool ReplaceCoopTextureIfTroughIsEmpty { get; set; } = true;

        /// <summary>
        /// The amount of friendship points player gets for watering a trough.
        /// </summary>
        public int FriendshipPointsForWateredTrough { get; set; } = 15;

        /// <summary>
        /// The amount of friendship points player gets for watering a trough with animals inside the building
        /// (i.e. when animals see the player watering the trough).
        /// </summary>
        public int AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding { get; set; } = 15;

        /// <summary>
        /// The amount of friendship points player loses for not watering a trough.
        /// </summary>
        public int NegativeFriendshipPointsForNotWateredTrough { get; set; } = 10;

        /// <summary>
        /// Whether animals can drink outside.
        /// </summary>
        public bool AnimalsCanDrinkOutside { get; set; } = true;

        /// <summary>
        /// Whether animals can only drink from lakes/rivers/seas etc. If set to false, animals will drink from any place you can refill your watering can at (well, troughs, water bodies etc.).
        /// </summary>
        public bool AnimalsCanOnlyDrinkFromWaterBodies { get; set; } = false;

        /// <summary>
        /// Whether troughs can remain full if all animals inside a building were able to drink outside during the day.
        /// </summary>
        public bool TroughsCanRemainFull { get; set; } = true;
        
        /// <summary>
        /// Whether troughs should have a cleaner texture.
        /// </summary>
        public bool CleanerTroughs { get; set; } = false;

        /// <summary>
        /// The price of the water bowl object at Marnie's animal shop. Defaults to zero.
        /// </summary>
        public int MarnieWaterBowlPrice { get; set; } = 0;
        
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
            
            api.AddSectionTitle(manifest, () => "Visual");
            api.AddBoolOption(manifest, () => config.ShowLoveBubblesOverAnimalsWhenWateredTrough, val => config.ShowLoveBubblesOverAnimalsWhenWateredTrough = val, () => "Show Love Bubbles", () => "Whether to show \"love\" bubbles over animals inside the building when watered the trough.");
            api.AddBoolOption(manifest, () => config.ShowAnimalsLeftThirstyMessage, val => config.ShowAnimalsLeftThirstyMessage = val, () => "Show Number of Animals Left Thirsty Message", () => "Whether to show a message (bottom left corner of screen) in the morning telling how many animals (if any) were left thirsty last night.");
            api.AddBoolOption(manifest, () => config.ReplaceCoopTextureIfTroughIsEmpty, val => config.ReplaceCoopTextureIfTroughIsEmpty = val, () => "Replace Coop Texture If Trough Is Empty", () => "Whether to replace coop's and big coop's textures when troughs inside them are empty.");
            api.AddBoolOption(manifest, () => config.CleanerTroughs, val => config.CleanerTroughs = val, () => "Cleaner Troughs", () => "Whether troughs should have a cleaner texture. Note: Won't change until a day update.");
            
            api.AddSectionTitle(manifest, () => "Functionality");
            api.AddBoolOption(manifest, () => config.UseWateringSystems, val => config.UseWateringSystems = val, () => "Use Watering Systems", () => "Whether to enable watering systems in certain buildings (e.g. Deluxe Barn, Deluxe Coop).");
            api.AddBoolOption(manifest, () => config.AnimalsCanDrinkOutside, val => config.AnimalsCanDrinkOutside = val, () => "Animals Can Drink Outside", () => "Whether animals can drink outside.");
            api.AddBoolOption(manifest, () => config.AnimalsCanOnlyDrinkFromWaterBodies, val => config.AnimalsCanOnlyDrinkFromWaterBodies = val, () => "Animals Can Only Drink From Water Bodies", () => "Whether animals can only drink from lakes/rivers/seas etc. If set to false, animals will drink from any place you can refill your watering can at (well, troughs, water bodies etc.).");
            api.AddBoolOption(manifest, () => config.TroughsCanRemainFull, val => config.TroughsCanRemainFull = val, () => "Troughs Can Remain Full", () => "Whether troughs can remain full if all animals inside a building were able to drink outside during the day.");

            api.AddSectionTitle(manifest, () => "Friendship");
            api.AddNumberOption(manifest, () => config.FriendshipPointsForWateredTrough, val => config.FriendshipPointsForWateredTrough = (int)val, () => "Watered Trough", () => "The amount of friendship points player gets for watering a trough.", interval: 1.0f);
            api.AddNumberOption(manifest, () => config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding, val => config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding = (int)val, () => "Watered Trough With Animals Inside", () => "The amount of friendship points player gets for watering a trough with animals inside the building (i.e. when animals see the player watering the trough).", interval: 1.0f);
            api.AddNumberOption(manifest, () => config.NegativeFriendshipPointsForNotWateredTrough, val => config.NegativeFriendshipPointsForNotWateredTrough = (int)val, () => "Negative; Not Watered Trough", () => "The amount of friendship points player loses for not watering a trough.", interval: 1.0f);
            
            api.AddSectionTitle(manifest, () => "Miscellaneous");
            api.AddNumberOption(manifest, () => config.MarnieWaterBowlPrice, val => config.MarnieWaterBowlPrice = (int)val, () => "Animal Shop Water Bowl Price", () => "The price of the water bowl object at Marnie's animal shop.", interval: 1.0f);
        }
    }
}
