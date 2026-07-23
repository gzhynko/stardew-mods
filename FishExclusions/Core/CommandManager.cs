using System;
using System.Linq;
using FishExclusions.Core.Game;
using StardewModdingAPI;
using StardewValley;

namespace FishExclusions.Core;

public static class CommandManager
{
    public static void RegisterCommands(IModHelper helper)
    {
        helper.ConsoleCommands.Add("fex_toggle", "Toggle exclusions.", Toggle);
        helper.ConsoleCommands.Add("fex_reload", "Reload the mod config.", ReloadConfig);
        helper.ConsoleCommands.Add("fex_list",
            "List the exclusions active for the current location, season and weather.", ListActiveExclusions);
        helper.ConsoleCommands.Add("fex_add",
            "Add an item to the common exclusions.\n\nUsage: fex_add <item id or name>", AddCommonExclusion);
        helper.ConsoleCommands.Add("fex_remove",
            "Remove an item from the common exclusions.\n\nUsage: fex_remove <item id or name>", RemoveCommonExclusion);
    }

    private static void Toggle(string command, string[] args)
    {
        ModEntry.ExclusionsEnabled = !ModEntry.ExclusionsEnabled;
        ModEntry.ModMonitor.Log($"Exclusions {(ModEntry.ExclusionsEnabled ? "enabled" : "disabled")}.", LogLevel.Info);
    }

    private static void ReloadConfig(string command, string[] args)
    {
        ModEntry.ReloadConfig();
        ModEntry.ModMonitor.Log("Config has been reloaded.", LogLevel.Info);
    }

    private static void ListActiveExclusions(string command, string[] args)
    {
        if (!Context.IsWorldReady || Game1.currentLocation is null)
        {
            ModEntry.ModMonitor.Log("please load a save first.", LogLevel.Info);
            return;
        }

        var location = Game1.currentLocation;
        var weather = location.GetWeather();
        var excluded =
            ExclusionResolver.GetExcludedFish(ModEntry.Config, location.GetSeasonKey(), location.Name, weather);

        var header = $"Status: {(ModEntry.ExclusionsEnabled ? "enabled" : "disabled")}. " +
                     $"Active exclusions for {location.Name} ({location.GetSeasonKey()}, {weather.Weather}):";

        if (excluded.Count == 0)
        {
            ModEntry.ModMonitor.Log($"{header} none.", LogLevel.Info);
            return;
        }

        var lines = excluded.Select(entry => $"- {entry}{DescribeEntry(entry)}");
        ModEntry.ModMonitor.Log($"{header}\n{string.Join("\n", lines)}", LogLevel.Info);
    }

    private static void AddCommonExclusion(string command, string[] args)
    {
        if (args.Length == 0)
        {
            ModEntry.ModMonitor.Log("Usage: fex_add <item id or name>", LogLevel.Info);
            return;
        }

        // join so internal names with spaces (e.g. "Rainbow Trout") work without quotes
        var entry = string.Join(" ", args);
        var config = ModEntry.Config;

        if (config.ItemsToExclude.CommonExclusions.Any(existing => EntriesMatch(existing, entry)))
        {
            ModEntry.ModMonitor.Log($"{entry} is already in the common exclusions.", LogLevel.Info);
            return;
        }

        config.ItemsToExclude.CommonExclusions = config.ItemsToExclude.CommonExclusions.Append(entry).ToArray();
        ModEntry.ModHelper.WriteConfig(config);

        ModEntry.ModMonitor.Log($"Added {entry}{DescribeEntry(entry)} to common exclusions.", LogLevel.Info);
    }

    private static void RemoveCommonExclusion(string command, string[] args)
    {
        if (args.Length == 0)
        {
            ModEntry.ModMonitor.Log("Usage: fex_remove <item id or name>", LogLevel.Info);
            return;
        }

        var entry = string.Join(" ", args);
        var config = ModEntry.Config;

        var remaining = config.ItemsToExclude.CommonExclusions.Where(existing => !EntriesMatch(existing, entry))
            .ToArray();
        if (remaining.Length == config.ItemsToExclude.CommonExclusions.Length)
        {
            ModEntry.ModMonitor.Log(
                $"{entry} is not in the common exclusions. If it is a conditional exclusion, edit config.json instead.",
                LogLevel.Info);
            return;
        }

        config.ItemsToExclude.CommonExclusions = remaining;
        ModEntry.ModHelper.WriteConfig(config);

        ModEntry.ModMonitor.Log($"Removed {entry} from the common exclusions.", LogLevel.Info);
    }

    private static bool EntriesMatch(string a, string b)
    {
        if (a.Equals(b, StringComparison.OrdinalIgnoreCase)) return true;

        var qualifiedA = ItemRegistry.QualifyItemId(a);
        return qualifiedA is not null &&
               qualifiedA.Equals(ItemRegistry.QualifyItemId(b), StringComparison.OrdinalIgnoreCase);
    }

    private static string DescribeEntry(string entry)
    {
        var data = ItemRegistry.GetData(entry);
        if (data is not null) return $" ({data.DisplayName})";

        // not an item id - try matching by obj name
        var byName = Game1.objectData.FirstOrDefault(pair => pair.Value.Name == entry);
        if (byName.Key is not null) return $" (name of item {ItemRegistry.QualifyItemId(byName.Key)})";

        return " (doesn't match any item id or internal item name)";
    }
}