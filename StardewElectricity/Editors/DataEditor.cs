using System.Collections.Generic;
using StardewElectricity.Buildings;
using StardewModdingAPI;

namespace StardewElectricity.Editors
{
    public class DataEditor : IAssetEditor
    {
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data/Blueprints");
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/Blueprints"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                data.Add(UtilityPole.BlueprintName,
                    "388 50/1/1/-1/-1/-1/-1/null/Utility Pole/Allows for electricity transmission within your farm's bounds. Built instantly./Buildings/none/64/110/-1/null/Farm/1000/true");
            }
        }
    }
}