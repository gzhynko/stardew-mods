using System;
using System.Collections.Generic;
using System.Linq;
using CropGrowthAdjustments.Core.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace CropGrowthAdjustments.Core.Content;

public class ContentPackLoader
{
    public List<Adjustments> ContentPacks = new();

    private const string ContentJsonName = "adjustments.json";
    private const int SpriteStripWidth = 128;
    private const int SpriteStripHeight = 32;

    public void LoadContentPacks(IModHelper helper, IMonitor monitor)
    {
        monitor.Log("Loading content packs...", LogLevel.Info);

        foreach (var contentPack in helper.ContentPacks.GetOwned())
        {
            if (!contentPack.HasFile(ContentJsonName))
            {
                monitor.Log(
                    $"  {contentPack.Manifest.Name} - Incorrect content pack folder structure. Expected {ContentJsonName} to be present in the folder.",
                    LogLevel.Error);
                continue;
            }

            List<CropAdjustment>? cropAdjustments;
            try
            {
                cropAdjustments = contentPack.ReadJsonFile<List<CropAdjustment>>(ContentJsonName);
            }
            catch (Exception e)
            {
                monitor.Log($"  {contentPack.Manifest.Name} - Error while parsing {ContentJsonName}: {e}", LogLevel.Error);
                continue;
            }

            if (cropAdjustments == null || cropAdjustments.Count == 0)
            {
                monitor.Log($"  {contentPack.Manifest.Name} - {ContentJsonName} contains no adjustments.", LogLevel.Error);
                continue;
            }
            
            var validAdjustments = new List<CropAdjustment>();
            for (var i = 0; i < cropAdjustments.Count; i++)
            {
                if (TryValidateAdjustment(contentPack, cropAdjustments[i], i, monitor))
                    validAdjustments.Add(cropAdjustments[i]);
            }

            if (validAdjustments.Count == 0)
            {
                monitor.Log($"  {contentPack.Manifest.Name} - No valid adjustments found, skipping this content pack.", LogLevel.Error);
                continue;
            }

            ContentPacks.Add(new Adjustments { ContentPack = contentPack, CropAdjustments = validAdjustments });
            monitor.Log($"  Loaded {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}: {contentPack.Manifest.Description}", LogLevel.Info);

        }

        if (ContentPacks.Count == 0)
        {
            monitor.Log("  No content packs to load.", LogLevel.Info);
        }
    }
    
    private bool TryValidateAdjustment(IContentPack contentPack, CropAdjustment adjustment, int index, IMonitor monitor)
    {
        var label = $"  {contentPack.Manifest.Name} - adjustment #{index + 1} ({adjustment.CropProduceName ?? adjustment.CropProduceItemId})";

        if (string.IsNullOrWhiteSpace(adjustment.CropProduceName) && adjustment.CropProduceItemId == "-1")
        {
            monitor.Log($"{label}: either CropProduceName or CropProduceItemId must be specified. Skipping this adjustment.", LogLevel.Error);
            return false;
        }

        if (!TryParseSeasons(adjustment.SeasonsToGrowIn, out var seasonsToGrowIn, out var error))
        {
            monitor.Log($"{label}: invalid SeasonsToGrowIn: {error}. Skipping this adjustment.", LogLevel.Error);
            return false;
        }
        if (!TryParseSeasons(adjustment.SeasonsToProduceIn, out var seasonsToProduceIn, out error))
        {
            monitor.Log($"{label}: invalid SeasonsToProduceIn: {error}. Skipping this adjustment.", LogLevel.Error);
            return false;
        }

        adjustment.ParsedSeasonsToGrowIn = seasonsToGrowIn;
        adjustment.ParsedSeasonsToProduceIn = seasonsToProduceIn;
        
        if (adjustment.SeasonsToHibernateIn != null)
        {
            if (!TryParseSeasons(adjustment.SeasonsToHibernateIn, out var seasonsToHibernateIn, out error))
            {
                monitor.Log($"{label}: invalid SeasonsToHibernateIn: {error}. Skipping this adjustment.", LogLevel.Error);
                return false;
            }
            if (seasonsToHibernateIn.Any(season => seasonsToProduceIn.Contains(season)))
            {
                monitor.Log($"{label}: a season cannot be both in SeasonsToProduceIn and SeasonsToHibernateIn. Skipping this adjustment.", LogLevel.Error);
                return false;
            }
            adjustment.ParsedSeasonsToHibernateIn = seasonsToHibernateIn;
        }
        
        adjustment.ParsedLocationsWithDefaultBehavior = SplitCommaList(adjustment.LocationsWithDefaultSeasonBehavior);

        adjustment.SpecialSpritesForSeasons ??= new List<SpecialSprites>();
        adjustment.SpecialSpritesForSeasons = adjustment.SpecialSpritesForSeasons
            .Where(sprites => TryValidateSpecialSprites(contentPack, sprites, label, monitor))
            .ToList();

        return true;
    }

    private bool TryValidateSpecialSprites(IContentPack contentPack, SpecialSprites sprites, string label, IMonitor monitor)
    {
        if (!TryParseSeasons(sprites.Season, out var seasons, out var error) || seasons.Count != 1)
        {
            monitor.Log($"{label}: invalid Season in SpecialSpritesForSeasons: {error ?? "expected exactly one season"}. Skipping these sprites.", LogLevel.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(sprites.Sprites))
        {
            monitor.Log($"{label}: an entry in SpecialSpritesForSeasons has no Sprites path. Skipping these sprites.", LogLevel.Error);
            return false;
        }

        if (!contentPack.HasFile(sprites.Sprites))
        {
            monitor.Log($"{label}: sprites file '{sprites.Sprites}' does not exist in the content pack. Skipping these sprites.", LogLevel.Error);
            return false;
        }

        Texture2D texture;
        try
        {
            texture = contentPack.ModContent.Load<Texture2D>(sprites.Sprites);
        }
        catch (Exception e)
        {
            monitor.Log($"{label}: could not load sprites file '{sprites.Sprites}': {e}. Skipping these sprites.", LogLevel.Error);
            return false;
        }

        if (texture.Width != SpriteStripWidth || texture.Height != SpriteStripHeight)
        {
            monitor.Log($"{label}: sprites file '{sprites.Sprites}' must be exactly {SpriteStripWidth}x{SpriteStripHeight} pixels, got {texture.Width}x{texture.Height}. Skipping these sprites.", LogLevel.Error);
            return false;
        }

        sprites.ParsedSeason = seasons[0];
        sprites.ParsedLocationsToIgnore = SplitCommaList(sprites.LocationsToIgnore);
        sprites.SpritesTexture = texture;

        return true;
    }
    
    private static bool TryParseSeasons(string? seasonsString, out List<Season> seasons, out string? error)
    {
        seasons = new List<Season>();
        error = null;

        if (string.IsNullOrWhiteSpace(seasonsString))
        {
            error = "no seasons specified";
            return false;
        }

        foreach (var part in seasonsString.Split(','))
        {
            switch (part.Trim().ToLower())
            {
                case "spring": seasons.Add(Season.Spring); break;
                case "summer": seasons.Add(Season.Summer); break;
                case "fall": seasons.Add(Season.Fall); break;
                case "winter": seasons.Add(Season.Winter); break;
                default:
                    error = $"unknown season '{part.Trim()}'";
                    return false;
            }
        }

        return true;
    }
    
    private static List<string> SplitCommaList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new List<string>();
        return value.Split(',').Select(e => e.Trim()).Where(e => e.Length > 0).ToList();
    }
}