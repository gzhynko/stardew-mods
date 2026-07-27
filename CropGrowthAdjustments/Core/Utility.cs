using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;

namespace CropGrowthAdjustments.Core;

public class Utility
{
    public static string GetItemIdByName(string itemName, IModHelper helper)
    {
        var objectData = helper.GameContent.Load<Dictionary<string, ObjectData>>("Data/Objects");

        foreach (var objectEntry in objectData)
        {
            if (CompareTwoStringsCaseAndSpaceIndependently(objectEntry.Value.Name, itemName))
            {
                return objectEntry.Key;
            }
        }

        return "-1";
    }

    public static bool CompareTwoStringsCaseAndSpaceIndependently(string first, string second)
    {
        return RemoveWhitespaceInString(first.ToLower()) == RemoveWhitespaceInString(second.ToLower());
    }

    public static string RemoveWhitespaceInString(string str)
    {
        return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
    }

    public static bool IsInAnyOfSpecifiedLocations(List<string> locations, GameLocation environment)
    {
        foreach (var location in locations)
        {
            switch (RemoveWhitespaceInString(location.ToLower()))
            {
                case "indoors":
                    if (!environment.IsOutdoors) return true;
                    break;
                default:
                    if (CompareTwoStringsCaseAndSpaceIndependently(location, environment.Name)) return true;
                    break;
            }
        }

        return false;
    }

    public static string NormalizeItemId(string itemId)
    {
        itemId = itemId.Trim();
        return itemId.StartsWith("(O)") ? itemId.Substring(3) : itemId;
    }
}