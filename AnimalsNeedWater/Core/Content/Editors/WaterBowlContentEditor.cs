using AnimalsNeedWater.Core.Models;
using StardewModdingAPI.Events;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Shops;

namespace AnimalsNeedWater.Core.Content.Editors;

public class WaterBowlContentEditor
{
    public static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        // add the water bowl item
        if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
        {
            e.Edit(asset =>
            {
                var editor = asset.AsDictionary<string, BigCraftableData>();

                var waterBowlAssetData = new BigCraftableData
                {
                    Name = "Water Bowl",
                    DisplayName = $"[LocalizedText Strings\\\\BigCraftables:{ModEntry.ModHelper.ModContent.ModID}_Items.Water_Bowl.name]",
                    Description = $"[LocalizedText Strings\\\\BigCraftables:{ModEntry.ModHelper.ModContent.ModID}_Items.Water_Bowl.description]",
                    Texture = ModEntry.ModHelper.ModContent.GetInternalAssetName(AssetManager.WaterBowlTextureSpritesheet).Name,
                    Price = 0, // sells for 0
                };
                
                editor.Data[ModConstants.WaterBowlItemId] = waterBowlAssetData;
            });
        }
        
        // add the translations for the bowl
        if (e.NameWithoutLocale.IsEquivalentTo("Strings/BigCraftables"))
        {
            e.Edit(asset =>
            {
                var editor = asset.AsDictionary<string, string>();
                editor.Data[$"{ModEntry.ModHelper.ModContent.ModID}_Items.Water_Bowl.name"] = 
                    ModEntry.ModHelper.Translation.Get("Items.Water_Bowl.name");
                editor.Data[$"{ModEntry.ModHelper.ModContent.ModID}_Items.Water_Bowl.description"] =  
                    ModEntry.ModHelper.Translation.Get("Items.Water_Bowl.description");
            });
        }
        
        // add the water bowl to Marnie's shop inventory
        if (ModEntry.Config.MarnieSellsWaterBowl && e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
        {
            e.Edit(asset =>
            {
                var editor = asset.AsDictionary<string, ShopData>();

                if (!editor.Data.TryGetValue("AnimalShop", out var animalShopData))
                    return;

                var waterBowlShopItemData = new ShopItemData
                {
                    Id = ModConstants.WaterBowlItemId,
                    ItemId = ModConstants.WaterBowlItemId,
                    Price = ModEntry.Config.MarnieWaterBowlPrice,
                };
                animalShopData.Items.Add(waterBowlShopItemData);
            });
        }
    }
}