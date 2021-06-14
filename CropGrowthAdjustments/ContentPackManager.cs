using System;
using System.Collections.Generic;
using CropGrowthAdjustments.Types;
using Newtonsoft.Json;
using StardewModdingAPI;

namespace CropGrowthAdjustments
{
    public class ContentPackManager
    {
        public List<Adjustments> ContentPacks = new List<Adjustments>();

        private const string ContentJsonName = "adjustments.json";

        public void InitializeContentPacks(IModHelper helper, IMonitor monitor)
        {
            foreach (var contentPack in helper.ContentPacks.GetOwned())
            {
                if (!contentPack.HasFile(ContentJsonName))
                {
                    monitor.Log(
                        $"[{contentPack.Manifest.Name}] - Incorrect content pack structure. Expected {ContentJsonName} to be present in the folder.");
                    continue;
                }

                var content = contentPack.ReadJsonFile<Adjustments>(ContentJsonName);
                content.ContentPack = contentPack;

                monitor.Log(content.CropAdjustments[0].CropProduceName, LogLevel.Info);

                ContentPacks.Add(content);
                monitor.VerboseLog($"Loaded {contentPack.Manifest.Name} by {contentPack.Manifest.Author}.");
            }

            if (ContentPacks.Count == 0)
            {
                monitor.VerboseLog("Nothing to load.");
            }
        }

        public void AssignCropProduceItemIds(IModHelper helper, IJsonAssetsApi jsonAssetsApi)
        {
            foreach (var contentPack in ContentPacks)
            {
                foreach (var adjustment in contentPack.CropAdjustments)
                {
                    adjustment.CropProduceItemId = Utility.GetItemIdByName(adjustment.CropProduceName, helper);

                    if (adjustment.CropProduceItemId == -1)
                    {
                        if (jsonAssetsApi == null) continue;
                        
                        adjustment.CropProduceItemId = jsonAssetsApi.GetObjectId(adjustment.CropProduceName);
                    }
                }
            }
        }

        public void AssignCropOriginalRowsInSpritesheet(IModHelper helper)
        {
            foreach (var contentPack in ContentPacks)
            {
                foreach (var adjustment in contentPack.CropAdjustments)
                {
                    if(adjustment.SpecialSpritesForSeasons.Count == 0) continue;
                    
                    var cropData =  Utility.GetCropDataForProduceItemId(adjustment.CropProduceItemId, helper);
                    if(cropData == null)
                    {
                        ModEntry.ModMonitor.Log($"[{contentPack.ContentPack.Manifest.Name}] - Unable to get the original row in spritesheet for {adjustment.CropProduceName}. Special sprites won't work.", LogLevel.Error);
                        continue;
                    }
                    
                    adjustment.OriginalRowInSpriteSheet = int.Parse(cropData[2]);
                }
            }

            ModEntry.ModMonitor.Log("assignCropOriginalRowsInSpritesheet", LogLevel.Info);
        }
    }
}