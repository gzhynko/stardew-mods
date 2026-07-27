using System.Collections.Generic;
using AnimalsNeedWater.Core.Models;
using StardewModdingAPI;

namespace AnimalsNeedWater.Core.Content;

internal static class CustomFieldsReader
{
    public static string Key(string key) => ModConstants.CustomFieldsPrefix + key;

    private static bool TryGetRaw(Dictionary<string, string>? fields, string key, out string value)
    {
        value = null!;
        return fields != null && fields.TryGetValue(ModConstants.CustomFieldsPrefix + key, out value!);
    }

    public static bool GetBool(Dictionary<string, string>? fields, string key, bool fallback)
    {
        if (!TryGetRaw(fields, key, out var raw)) return fallback;
        if (bool.TryParse(raw.Trim(), out var parsed)) return parsed;
        Warn(key, raw, "bool");
        return fallback;
    }

    public static int GetInt(Dictionary<string, string>? fields, string key, int fallback)
    {
        if (!TryGetRaw(fields, key, out var raw)) return fallback;
        if (int.TryParse(raw.Trim(), out var parsed)) return parsed;
        Warn(key, raw, "int");
        return fallback;
    }

    public static int? GetIntNullable(Dictionary<string, string>? fields, string key)
    {
        if (!TryGetRaw(fields, key, out var raw)) return null;
        if (int.TryParse(raw.Trim(), out var parsed)) return parsed;
        Warn(key, raw, "int");
        return null;
    }

    private static void Warn(string key, string raw, string expectedType)
        => ModEntry.ModMonitor.Log(
            $"ignoring malformed value \"{raw}\" for CustomField \"{ModConstants.CustomFieldsPrefix + key}\" (expected type: {expectedType}).",
            LogLevel.Warn);
}