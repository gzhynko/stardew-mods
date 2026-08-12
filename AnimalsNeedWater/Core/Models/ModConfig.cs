using System;
using StardewModdingAPI;
using StardewValley;

namespace AnimalsNeedWater.Core.Models;

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
    
    /// <summary>
    /// Whether to show the current thirst status on an animal's mood screen (the one you get when you right click an animal). 
    /// </summary>
    public bool ShowMoodThirstyStatus { get; set; } = true;
    
    [Obsolete("Use UseWateringSystems instead")]
    public bool? WateringSystemInDeluxeBuildings {
        get => null;
        set { if (value.HasValue) UseWateringSystems = value.Value; }
    }
    public bool ShouldSerializeWateringSystemInDeluxeBuildings() => false;

    /// <summary>
    /// Whether to enable watering systems in certain buildings (e.g. Deluxe Barn, Deluxe Coop).
    /// </summary>
    public bool UseWateringSystems { get; set; } = true;

    [Obsolete("Use ChangeBuildingTextureIfTroughIsEmpty instead")]
    public bool? ReplaceCoopTextureIfTroughIsEmpty
    {
        get => null;
        set { if (value.HasValue) ChangeBuildingTextureIfTroughIsEmpty = value.Value; }
    }
    public bool ShouldSerializeReplaceCoopTextureIfTroughIsEmpty() => false;

    /// <summary>
    /// Whether to change building textures (e.g., coop and big coop) when troughs inside are empty.
    /// </summary>
    public bool ChangeBuildingTextureIfTroughIsEmpty { get; set; } = true;

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
    /// Whether in-building water sources (troughs, bowls) can remain full if all animals living in that building were able to drink outside during the day.
    /// </summary>
    public bool TroughsCanRemainFull { get; set; } = true;
        
    /// <summary>
    /// Whether troughs should have a cleaner texture.
    /// </summary>
    public bool CleanerTroughs { get; set; } = false;

    /// <summary>
    /// Whether Marnie sells the water bowl. Defaults to true.
    /// </summary>
    public bool MarnieSellsWaterBowl { get; set; } = true;
    
    /// <summary>
    /// The price of the water bowl object at Marnie's animal shop. Defaults to zero.
    /// </summary>
    public int MarnieWaterBowlPrice { get; set; } = 0;
    
    /// <summary>
    /// Whether to indicate that an animal is still thirsty by periodically showing an emote above it. Defaults to true.
    /// </summary>
    public bool ShowThirstyEmotes { get; set; } = true;
    
    /// <summary>
    /// The time cutoff in HHMM after which animals can start showing emotes indicating they are still thirsty. Defaults to 1500 (3pm).
    /// </summary>
    public int ThirstyEmoteStartTime { get; set; } = 1500;

    /// <summary>
    /// Whether to show a tooltip when hovering over a water trough, watering system, or water source object,
    /// indicating whether it needs refilling.
    /// </summary>
    public bool ShowWaterSourceTooltip { get; set; } = true;

    /// <summary>
    /// Whether sprinklers can fill water bowls on day start. Defaults to true.
    /// </summary>
    public bool SprinklersFillWaterBowls { get; set; } = true;

    /// <summary>
    /// Setup the Generic Mod Config Menu API.
    /// </summary>
    public static void SetUpModConfigMenu(ModConfig config, ModEntry mod)
    {
        IGenericModConfigMenuApi? api = mod.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (api == null) return;

        var manifest = mod.ModManifest;

        Func<string> T(string key) => () => ModEntry.ModHelper.Translation.Get(key);

        api.Register(manifest, () =>
        {
            config = new ModConfig();
            mod.SaveConfig(config);
        }, () => mod.SaveConfig(config));

        api.AddSectionTitle(manifest, T("Config.Section.Appearance"));
        api.AddBoolOption(manifest, () => config.ChangeBuildingTextureIfTroughIsEmpty, val => config.ChangeBuildingTextureIfTroughIsEmpty = val, T("Config.ChangeBuildingTextureIfTroughIsEmpty.name"), T("Config.ChangeBuildingTextureIfTroughIsEmpty.tooltip"));
        api.AddBoolOption(manifest, () => config.CleanerTroughs, val => config.CleanerTroughs = val, T("Config.CleanerTroughs.name"), T("Config.CleanerTroughs.tooltip"));

        api.AddSectionTitle(manifest, T("Config.Section.Notifications"));
        api.AddBoolOption(manifest, () => config.ShowLoveBubblesOverAnimalsWhenWateredTrough, val => config.ShowLoveBubblesOverAnimalsWhenWateredTrough = val, T("Config.ShowLoveBubblesOverAnimalsWhenWateredTrough.name"), T("Config.ShowLoveBubblesOverAnimalsWhenWateredTrough.tooltip"));
        api.AddBoolOption(manifest, () => config.ShowAnimalsLeftThirstyMessage, val => config.ShowAnimalsLeftThirstyMessage = val, T("Config.ShowAnimalsLeftThirstyMessage.name"), T("Config.ShowAnimalsLeftThirstyMessage.tooltip"));
        api.AddBoolOption(manifest, () => config.ShowMoodThirstyStatus, val => config.ShowMoodThirstyStatus = val, T("Config.ShowMoodThirstyStatus.name"), T("Config.ShowMoodThirstyStatus.tooltip"));
        api.AddBoolOption(manifest, () => config.ShowThirstyEmotes, val => config.ShowThirstyEmotes = val, T("Config.ShowThirstyEmotes.name"), T("Config.ShowThirstyEmotes.tooltip"));
        api.AddNumberOption(manifest,
            () => config.ThirstyEmoteStartTime,
            val => config.ThirstyEmoteStartTime = (int)val,
            T("Config.ThirstyEmoteStartTime.name"),
            T("Config.ThirstyEmoteStartTime.tooltip"),
            min: 600f, max: 2600f, interval: 100f,
            formatValue: val => Game1.getTimeOfDayString((int)val));
        api.AddBoolOption(manifest,
            () => config.ShowWaterSourceTooltip,
            val => config.ShowWaterSourceTooltip = val,
            T("Config.ShowWaterSourceTooltip.name"),
            T("Config.ShowWaterSourceTooltip.tooltip"));

        api.AddSectionTitle(manifest, T("Config.Section.WaterSources"));
        api.AddBoolOption(manifest, () => config.UseWateringSystems, val => config.UseWateringSystems = val, T("Config.UseWateringSystems.name"), T("Config.UseWateringSystems.tooltip"));
        api.AddBoolOption(manifest, () => config.AnimalsCanDrinkOutside, val => config.AnimalsCanDrinkOutside = val, T("Config.AnimalsCanDrinkOutside.name"), T("Config.AnimalsCanDrinkOutside.tooltip"));
        api.AddBoolOption(manifest, () => config.AnimalsCanOnlyDrinkFromWaterBodies, val => config.AnimalsCanOnlyDrinkFromWaterBodies = val, T("Config.AnimalsCanOnlyDrinkFromWaterBodies.name"), T("Config.AnimalsCanOnlyDrinkFromWaterBodies.tooltip"));
        api.AddBoolOption(manifest, () => config.TroughsCanRemainFull, val => config.TroughsCanRemainFull = val, T("Config.TroughsCanRemainFull.name"), T("Config.TroughsCanRemainFull.tooltip"));
        api.AddBoolOption(manifest, () => config.SprinklersFillWaterBowls, val => config.SprinklersFillWaterBowls = val, T("Config.SprinklersFillWaterBowls.name"), T("Config.SprinklersFillWaterBowls.tooltip"));

        api.AddSectionTitle(manifest, T("Config.Section.Friendship"));
        api.AddNumberOption(manifest, () => config.FriendshipPointsForWateredTrough, val => config.FriendshipPointsForWateredTrough = (int)val, T("Config.FriendshipPointsForWateredTrough.name"), T("Config.FriendshipPointsForWateredTrough.tooltip"), interval: 1.0f);
        api.AddNumberOption(manifest, () => config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding, val => config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding = (int)val, T("Config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding.name"), T("Config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding.tooltip"), interval: 1.0f);
        api.AddNumberOption(manifest, () => config.NegativeFriendshipPointsForNotWateredTrough, val => config.NegativeFriendshipPointsForNotWateredTrough = (int)val, T("Config.NegativeFriendshipPointsForNotWateredTrough.name"), T("Config.NegativeFriendshipPointsForNotWateredTrough.tooltip"), interval: 1.0f);

        api.AddSectionTitle(manifest, T("Config.Section.Miscellaneous"));
        api.AddBoolOption(manifest, () => config.MarnieSellsWaterBowl, val => config.MarnieSellsWaterBowl = val, T("Config.MarnieSellsWaterBowl.name"), T("Config.MarnieSellsWaterBowl.tooltip"));
        api.AddNumberOption(manifest, () => config.MarnieWaterBowlPrice, val => config.MarnieWaterBowlPrice = (int)val, T("Config.MarnieWaterBowlPrice.name"), T("Config.MarnieWaterBowlPrice.tooltip"), interval: 1.0f);
    }
}