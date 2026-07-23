using System.Collections.Generic;
using CropGrowthAdjustments.Core.Models;
using StardewModdingAPI;
using StardewValley.GameData.Crops;

namespace CropGrowthAdjustments.Core.Content;

public class AdjustmentResolver
{
    public void ResolveAll(IModHelper helper)
    {
        ResolveProduceItemIds(helper);
        ResolveCropRows(helper);
        helper.GameContent.InvalidateCache("Data/Crops");
    }

    private void ResolveProduceItemIds(IModHelper helper)
    {
        foreach (var contentPack in ModEntry.ContentPackLoader.ContentPacks)
        {
            foreach (var adjustment in contentPack.CropAdjustments)
            {
                // skip if the item id is specified by the content pack or already resolved
                if (adjustment.CropProduceItemId != "-1") continue;

                adjustment.CropProduceItemId = Utility.GetItemIdByName(adjustment.CropProduceName!, helper);

                // warn the player if the ID is still not assigned
                // this means that either the crop produce name is specified incorrectly OR that the crop produce name was edited
                if (adjustment.CropProduceItemId == "-1")
                {
                    ModEntry.ModMonitor.Log($"{contentPack.ContentPack.Manifest.Name} - Unable to assign ID to {adjustment.CropProduceName}. " +
                                            "Make sure the name you specified matches the desired crop name. Otherwise, if this crop had its produce item name " +
                                            "edited (e.g. via ContentPatcher), make sure to specify the CropProduceItemId in adjustments.json.", LogLevel.Warn);
                    continue;
                }

                ModEntry.ModMonitor.Log($"Assigned item id {adjustment.CropProduceItemId} to {adjustment.CropProduceName}.");
            }
        }
    }

    private void ResolveCropRows(IModHelper helper)
    {
        var cropData = helper.GameContent.Load<Dictionary<string, CropData>>("Data/Crops");

        foreach (var contentPack in ModEntry.ContentPackLoader.ContentPacks)
        {
            foreach (var adjustment in contentPack.CropAdjustments)
            {
                if (adjustment.SpecialSpritesForSeasons.Count == 0) continue;
                // unresolved produce id; ResolveProduceItemIds already warned about it
                if (adjustment.CropProduceItemId == "-1") continue;

                var entry = GetCropDataForProduceItemId(cropData, adjustment.CropProduceItemId);
                if (entry == null)
                {
                    ModEntry.ModMonitor.Log($"{contentPack.ContentPack.Manifest.Name} - Unable to get the crop data for {adjustment.CropProduceName}. Special sprites won't work.", LogLevel.Error);
                    continue;
                }

                adjustment.RowInCropSpriteSheet = entry.SpriteIndex;
            }
        }
    }

    private static CropData? GetCropDataForProduceItemId(Dictionary<string, CropData> cropData, string produceId)
    {
        var normalizedProduceId = Utility.NormalizeItemId(produceId);

        foreach (var entry in cropData.Values)
        {
            if (entry.HarvestItemId == null) continue;
            if (Utility.NormalizeItemId(entry.HarvestItemId) != normalizedProduceId) continue;

            return entry;
        }

        return null;
    }
}
