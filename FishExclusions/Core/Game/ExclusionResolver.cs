using System;
using System.Collections.Generic;
using FishExclusions.Core.Models;
using StardewValley;
using StardewValley.Network;

namespace FishExclusions.Core.Game;

internal static class ExclusionResolver
{
    public static HashSet<string> GetExcludedFish(ModConfig config, string seasonName, string locationName,
        LocationWeather weather)
    {
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // common exclusions always apply.
        foreach (var itemId in config.ItemsToExclude.CommonExclusions)
        {
            excluded.Add(itemId);
        }

        foreach (var exclusion in config.ItemsToExclude.ConditionalExclusions)
        {
            if (!Matches(exclusion, seasonName, locationName, weather)) continue;

            foreach (var itemId in exclusion.Exclusions)
            {
                excluded.Add(itemId);
            }
        }

        return excluded;
    }

    public static bool IsExcluded(Item? item, HashSet<string> excludedIds)
    {
        if (item is null) return false;
        // support both plain and qualified ids as well as names
        return excludedIds.Contains(item.ItemId) || excludedIds.Contains(item.QualifiedItemId) ||
               excludedIds.Contains(item.Name);
    }

    private static bool Matches(ConditionalExclusion exclusion, string seasonName, string locationName,
        LocationWeather weather)
    {
        // empty condition fields = wildcards
        if (exclusion.Season != "" &&
            !exclusion.Season.Equals(seasonName, StringComparison.OrdinalIgnoreCase)) return false;
        if (exclusion.Location != "" &&
            !exclusion.Location.Equals(locationName, StringComparison.OrdinalIgnoreCase)) return false;

        return MatchesWeather(exclusion.Weather, weather);
    }

    private static bool MatchesWeather(string condition, LocationWeather weather)
    {
        if (condition == "") return true;

        // support legacy values - rain = any rainy weather (incl. storms and green rain), sunny matches the rest
        if (condition.Equals("sunny", StringComparison.OrdinalIgnoreCase)) return !weather.IsRaining;
        if (condition.Equals("rain", StringComparison.OrdinalIgnoreCase)) return weather.IsRaining;

        // otherwise match the game weather id: Sun, Wind, Storm, GreenRain, Snow, Festival, Wedding...
        return condition.Equals(weather.Weather, StringComparison.OrdinalIgnoreCase);
    }
}