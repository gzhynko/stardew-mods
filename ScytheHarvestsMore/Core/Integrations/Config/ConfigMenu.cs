using ScytheHarvestsMore.Core.Models;

namespace ScytheHarvestsMore.Core.Integrations.Config;

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

        api.AddSectionTitle(manifest, () => "Functionality");

        api.AddBoolOption(manifest, () => config.HarvestMachines, val => config.HarvestMachines = val,
            () => "Harvest Machines",
            () => "Whether the iridium scythe harvests machines.");
        api.AddBoolOption(manifest, () => config.HarvestFruitTrees, val => config.HarvestFruitTrees = val,
            () => "Harvest Fruit Trees",
            () => "Whether the iridium scythe harvests fruit trees.");
        api.AddBoolOption(manifest, () => config.HarvestForage, val => config.HarvestForage = val,
            () => "Harvest Forage",
            () => "Whether the iridium scythe harvests forage items.");
        api.AddBoolOption(manifest, () => config.HarvestGinger, val => config.HarvestGinger = val,
            () => "Harvest Ginger",
            () => "Whether the iridium scythe digs up ginger crops.");
        api.AddBoolOption(manifest, () => config.HarvestBerryBushes, val => config.HarvestBerryBushes = val,
            () => "Harvest Berry Bushes",
            () => "Whether the iridium scythe harvests berry bushes.");
        api.AddBoolOption(manifest, () => config.ShakeTrees, val => config.ShakeTrees = val,
            () => "Shake Trees",
            () => "Whether the iridium scythe shakes grown wild trees, dropping their seeds or other items (e.g coconuts from coconut palms).");
        api.AddBoolOption(manifest, () => config.HarvestCrabPots, val => config.HarvestCrabPots = val,
            () => "Harvest Crab Pots",
            () => "Whether the iridium scythe harvests crab pots. The catch is added to your inventory.");
    }
}