using FishExclusions.Core.Models;

namespace FishExclusions.Core.Integrations.Config;

internal static class ConfigMenu
{
    public static void Register(ModConfig config, ModEntry mod)
    {
        var api = mod.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (api == null) return;

        var manifest = mod.ModManifest;

        api.Register(manifest, () =>
        {
            config = new ModConfig();
            mod.SaveConfig(config);
        }, () => mod.SaveConfig(config));

        api.AddSectionTitle(manifest, () => "General");

        api.AddTextOption(manifest, () => config.ItemToCatchIfAllFishIsExcluded,
            val => config.ItemToCatchIfAllFishIsExcluded = val, () => "Item To Catch If All Fish Is Excluded",
            () => "The ID of the item to catch if all possible fish for this location / season / weather is excluded.");
        api.AddNumberOption(manifest, () => config.TimesToRetry, val => config.TimesToRetry = val,
            () => "Times To Retry",
            () => "The number of times to retry fish selection before giving up and catching the item specified above.",
            5, 50);

        api.AddParagraph(manifest,
            () =>
                "To edit the actual excluded fish, use the config.json file located in [Stardew Valley folder]/Mods/FishExclusions. For instructions on how to add exclusions, refer to the mod description on Nexus. You can also use console commands to edit common exclusions (run \"help\" in SMAPI console and look for FishExclusions).");
    }
}