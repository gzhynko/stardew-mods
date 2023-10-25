using System;
using System.Collections.Generic;
using CropGrowthAdjustments.Types;
using StardewModdingAPI;

namespace CropGrowthAdjustments
{
    public class Utility
    {
        public static int GetItemIdByName(string itemName, IModHelper helper)
        {
            var objectData = helper.GameContent.Load<Dictionary<int, string>>("Data/ObjectInformation");

            foreach (var objectEntry in objectData)
            {
                if (CompareTwoStringsCaseAndSpaceIndependently(objectEntry.Value.Split('/')[0], itemName))
                {
                    return objectEntry.Key;
                }
            }

            return -1;
        }

        public static string[] GetCropDataForProduceItemId(int produceId, IModHelper helper)
        {
            var cropData = helper.GameContent.Load<Dictionary<int, string>>("Data/Crops");
            
            foreach (var itemId in cropData.Keys)
            {
                var itemData = cropData[itemId];
                var fields = itemData.Split('/');
                        
                if(int.Parse(fields[3]) != produceId) continue;

                return fields;
            }

            return null;
        }

        public static bool JsonAssetsHasCropsLoaded(IJsonAssetsApi jsonAssetsApi)
        {
            if (jsonAssetsApi == null) return false;

            return jsonAssetsApi.GetAllCropIds().Count != 0;
        }

        public static bool CompareTwoStringsCaseAndSpaceIndependently(string first, string second)
        {
            return RemoveWhitespaceInString(first.ToLower()) == RemoveWhitespaceInString(second.ToLower());
        }
        
        public static string RemoveWhitespaceInString(string str) {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }
    }
}