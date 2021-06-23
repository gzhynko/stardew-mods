using StardewElectricity.Buildings;
using StardewModdingAPI;

namespace StardewElectricity.Loaders
{
    public class AssetLoader : IAssetLoader
    {
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals($"Buildings\\{UtilityPole.BlueprintName}");
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals($"Buildings\\{UtilityPole.BlueprintName}"))
                return (T)(object)ModEntry.PoleTexture;

            return (T)(object)null;
        }
    }
}