using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AnimalsNeedWater.Core.Content.Models;
using AnimalsNeedWater.Core.Models;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.ItemTypeDefinitions;
using Object = StardewValley.Object;

namespace AnimalsNeedWater.Core.Content;

public class WaterSourceRegistry
{
    // keyed by qualified item id; null value means lookup was done but this is not a water source
    private readonly Dictionary<string, WaterSource?> _cache = new(StringComparer.OrdinalIgnoreCase);
    
    public void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(name => name.IsEquivalentTo("Data/BigCraftables")))
            _cache.Clear();
    }

    public bool TryGet(string? qualifiedItemId, [NotNullWhen(true)] out WaterSource? source)
    {
        source = null;
        if (string.IsNullOrEmpty(qualifiedItemId))
            return false;

        if (!_cache.TryGetValue(qualifiedItemId, out source))
        {
            source = Parse(qualifiedItemId);
            _cache[qualifiedItemId] = source;
        }

        return source != null;
    }

    public bool TryGet(Object obj, [NotNullWhen(true)] out WaterSource? source)
        => TryGet(obj.QualifiedItemId, out source);

    private static WaterSource? Parse(string qualifiedItemId)
    {
        ParsedItemData? data = ItemRegistry.GetData(qualifiedItemId);
        if (data?.RawData is not BigCraftableData bigCraftable)
            return null;

        var fields = bigCraftable.CustomFields;
        if (!CustomFieldsReader.GetBool(fields, ModConstants.CustomFieldIsWaterSourceKey, false))
            return null;

        return new WaterSource
        {
            NeedsRefilling = CustomFieldsReader.GetBool(fields, "NeedsRefilling", true),
            FillableByCan = CustomFieldsReader.GetBool(fields, "FillableByCan", true),
            FillableBySprinkler = CustomFieldsReader.GetBool(fields, "FillableBySprinkler", true),
            EmptySpriteIndex = CustomFieldsReader.GetInt(fields, "EmptySpriteIndex", 0),
            FullSpriteIndex = CustomFieldsReader.GetInt(fields, "FullSpriteIndex", 1),
        };
    }
    
    public bool IsFull(Object obj, WaterSource source)
    {
        if (!source.NeedsRefilling) return true;

        // new key is named agnostic of the particular object
        if (obj.modData.TryGetValue(ModConstants.ModDataWaterSourceFullKey, out var val) && val == "true")
        {
            return true;
        }
        // fallback in case this is a pre-1.8.2 water bowl
        return obj.modData.TryGetValue(ModConstants.WaterBowlItemModDataIsFullField, out var legacyVal) && legacyVal == "true";
    }

    public void Fill(Object obj, WaterSource source)
    {
        if (IsFull(obj, source)) return;
        obj.modData[ModConstants.ModDataWaterSourceFullKey] = "true";
        obj.modData.Remove(ModConstants.WaterBowlItemModDataIsFullField); // clear legacy flag
        obj.ParentSheetIndex = source.FullSpriteIndex;
    }

    public void Empty(Object obj, WaterSource source)
    {
        if (!source.NeedsRefilling) return;
        obj.modData[ModConstants.ModDataWaterSourceFullKey] = "false";
        obj.modData.Remove(ModConstants.WaterBowlItemModDataIsFullField);
        obj.ParentSheetIndex = source.EmptySpriteIndex;
    }
}
