using System;
using StardewModdingAPI;

namespace FishExclusions.Core.Integrations.Config;

public interface IGenericModConfigMenuApi
{
    void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);
    void AddSectionTitle(IManifest mod, Func<string> text, Func<string>? tooltip = null);

    void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name,
        Func<string>? tooltip = null, int? min = null, int? max = null, int? interval = null,
        Func<int, string>? formatValue = null, string? fieldId = null);

    void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name,
        Func<string>? tooltip = null, string[]? allowedValues = null, Func<string, string>? formatAllowedValue = null,
        string? fieldId = null);

    void AddParagraph(IManifest mod, Func<string> text);
}