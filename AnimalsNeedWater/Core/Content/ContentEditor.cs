using AnimalsNeedWater.Core.Models;
using StardewModdingAPI.Events;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Shops;

namespace AnimalsNeedWater.Core.Content;

public class ContentEditor
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
                    Price = ModEntry.Config.MarnieWaterBowlPrice,
                };
                editor.Data.Add(ModConstants.WaterBowlItemId, waterBowlAssetData);
            });
        }
        
        // add the translations for the bowl
        if (e.NameWithoutLocale.IsEquivalentTo("Strings/BigCraftables"))
        {
            e.Edit(asset =>
            {
                var editor = asset.AsDictionary<string, string>();
                editor.Data.Add(
                    $"{ModEntry.ModHelper.ModContent.ModID}_Items.Water_Bowl.name", 
                    ModEntry.ModHelper.Translation.Get("Items.Water_Bowl.name"));
                editor.Data.Add(
                    $"{ModEntry.ModHelper.ModContent.ModID}_Items.Water_Bowl.description", 
                    ModEntry.ModHelper.Translation.Get("Items.Water_Bowl.description"));
            });
        }
        
        // add the water bowl to Marnie's shop inventory
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
        {
            e.Edit(asset =>
            {
                var editor = asset.AsDictionary<string, ShopData>();

                var animalShopData = editor.Data["AnimalShop"];

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