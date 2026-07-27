using System.Collections.Generic;
using System.Linq;
using CropGrowthAdjustments.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CropGrowthAdjustments.Core.Content;

/// <summary>
/// this is what allows us to assign Crop.overrideTexturePath as full replacement texture. Crop.getsourcerect only reads from a specific row
/// within that texture, hence we put the special sprite at exactly that row
/// </summary>
public class SeasonalTextureFactory
{
    public const string AssetPrefix = "Mods/GZhynko.CropGrowthAdjustments";

    // registered generated assets keyed by asset name
    private readonly Dictionary<string, (SpecialSprites Sprites, int Row)> _assets = new();

    public void RegisterAssets(IModHelper helper)
    {
        _assets.Clear();

        foreach (var contentPack in ModEntry.ContentPackLoader.ContentPacks)
        {
            foreach (var adjustment in contentPack.CropAdjustments)
            {
                if (adjustment.RowInCropSpriteSheet is not { } row) continue;

                foreach (var sprites in adjustment.SpecialSpritesForSeasons)
                {
                    var assetName = $"{AssetPrefix}/{SanitizeForAssetName(contentPack.ContentPack.Manifest.UniqueID)}" +
                                    $"/{SanitizeForAssetName(Utility.NormalizeItemId(adjustment.CropProduceItemId))}/{sprites.ParsedSeason}";

                    sprites.GeneratedAssetName = assetName;
                    _assets[assetName] = (sprites, row);
                }
            }
        }

        // drop any stale textures from a previous resolution
        helper.GameContent.InvalidateCache(asset => asset.Name.BaseName.StartsWith(AssetPrefix));
    }

    /// <summary> called within OnAssetRequested to see if we should generate and serve a texture. This is called on every asset request, but the dict lookup is O(1) so the perf impact is minimal.</summary>
    public bool TryHandleAssetRequest(AssetRequestedEventArgs e)
    {
        if (!_assets.TryGetValue(e.NameWithoutLocale.BaseName, out var entry)) return false;

        e.LoadFrom(() => BuildTexture(entry.Sprites, entry.Row), AssetLoadPriority.Exclusive);
        return true;
    }

    /// <summary> build a texture that is transparent except for the sprite strip at the crop's row position.</summary>
    private static Texture2D BuildTexture(SpecialSprites sprites, int row)
    {
        const int stripWidth = 128;
        const int stripHeight = 32;

        var targetX = row % 2 == 0 ? 0 : stripWidth;
        var targetY = row / 2 * stripHeight;

        var width = stripWidth * 2;
        var height = targetY + stripHeight;

        var stripData = new Color[stripWidth * stripHeight];
        sprites.SpritesTexture.GetData(stripData);

        var textureData = new Color[width * height];
        for (var y = 0; y < stripHeight; y++)
        {
            System.Array.Copy(stripData, y * stripWidth, textureData, (targetY + y) * width + targetX, stripWidth);
        }

        var texture = new Texture2D(Game1.graphics.GraphicsDevice, width, height);
        texture.SetData(textureData);
        return texture;
    }

    private static string SanitizeForAssetName(string value)
    {
        return string.Concat(value.Select(c => char.IsLetterOrDigit(c) || c is '.' or '-' or '_' ? c : '_'));
    }
}
