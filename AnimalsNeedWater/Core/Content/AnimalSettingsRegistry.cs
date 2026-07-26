using System;
using System.Collections.Generic;
using System.Linq;
using AnimalsNeedWater.Core.Content.Models;
using StardewModdingAPI.Events;
using StardewValley;

namespace AnimalsNeedWater.Core.Content;

public class AnimalSettingsRegistry
{
    // keyed by animal type id (the Data/FarmAnimals key)
    private readonly Dictionary<string, AnimalSettings> _cache = new(StringComparer.OrdinalIgnoreCase);

    public void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(name => name.IsEquivalentTo("Data/FarmAnimals")))
            _cache.Clear();
    }
    
    public AnimalSettings For(string? animalType)
    {
        if (string.IsNullOrEmpty(animalType))
            return AnimalSettings.Default;

        if (!_cache.TryGetValue(animalType, out var settings))
        {
            settings = Parse(animalType);
            _cache[animalType] = settings;
        }

        return settings;
    }
    
    public AnimalSettings For(FarmAnimal animal) => For(animal.type.Value);

    private static AnimalSettings Parse(string animalType)
    {
        if (!Game1.farmAnimalData.TryGetValue(animalType, out var data) || data.CustomFields == null)
            return AnimalSettings.Default;

        var fields = data.CustomFields;
        return new AnimalSettings
        {
            NeedsWater = CustomFieldsReader.GetBool(fields, "NeedsWater", true),
            WaterIntervalDays = Math.Max(1, CustomFieldsReader.GetInt(fields, "WaterIntervalDays", 1)),
            CanDrinkOutside = CustomFieldsReader.GetBool(fields, "CanDrinkOutside", true),
            DailyWaterFriendship = CustomFieldsReader.GetIntNullable(fields, "DailyWaterFriendship"),
            WitnessedWaterFriendship = CustomFieldsReader.GetIntNullable(fields, "WitnessedWaterFriendship"),
            ThirstyFriendshipPenalty = CustomFieldsReader.GetIntNullable(fields, "ThirstyFriendshipPenalty"),
            ThirstyEmoteId = CustomFieldsReader.GetIntNullable(fields, "ThirstyEmoteId"),
            DrinkOutsideEmoteId = CustomFieldsReader.GetIntNullable(fields, "DrinkOutsideEmoteId"),
            WaterBonusEmoteId = CustomFieldsReader.GetIntNullable(fields, "WaterBonusEmoteId"),
        };
    }
}
