using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

// ReSharper disable InconsistentNaming

namespace CropGrowthAdjustments
{
    public class HarmonyPatches
    {
        /// <summary> Patch for the HoeDirt.dayUpdate </summary>
        public static bool HoeDirtDayUpdate(HoeDirt __instance, GameLocation environment, Vector2 tileLocation)
        {
            // Avoid running if no crop is planted.
            if (__instance.crop == null) return true;

            foreach (var contentPack in ModEntry.ContentPackManager.ContentPacks)
            {
                foreach (var adjustment in contentPack.CropAdjustments)
                {
                    // skip if this crop is not the desired one.
                    if (__instance.crop.indexOfHarvest != adjustment.CropProduceItemId) continue;

                    // run the original method if this crop is supposed to die in winter.
                    if (adjustment.GetSeasonsToGrowIn().All(season => season != "winter")) return true;
                    
                    // Skip the original method if the current season is winter to prevent the crop from dying (since all crops die in the winter).
                    if (Game1.GetSeasonForLocation(environment) == "winter")
                    {
                        __instance.crop.newDay((int) __instance.state, (int) __instance.fertilizer, (int) tileLocation.X, (int) tileLocation.Y, environment);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary> 
        /// Patch for the HoeDirt.draw method.
        /// 
        /// The performance hit doesn't seem to be too large, so I went with this for updating grape textures.
        /// </summary>
        public static void HoeDirtDraw(HoeDirt __instance, Vector2 tileLocation)
        {
            // Avoid running if no crop is planted.
            if (__instance.crop == null) return;
            
            // ModEntry.ModMonitor.Log($"indexOfHarvest: {__instance.crop.indexOfHarvest}, rowInSpritesheet: {__instance.crop.rowInSpriteSheet}", LogLevel.Info);

            foreach (var contentPack in ModEntry.ContentPackManager.ContentPacks)
            {
                foreach (var adjustment in contentPack.CropAdjustments)
                {
                    // skip if this crop is not the desired one.
                    if (__instance.crop.indexOfHarvest != adjustment.CropProduceItemId) continue;

                    foreach (var specialSprite in adjustment.SpecialSpritesForSeasons)
                    {
                        // skip if the crop is planted in any of the ignored locations.
                        if (specialSprite.GetLocationsToIgnore().Any(location => Utility.CompareTwoStringsCaseAndSpaceIndependently(location,__instance.currentLocation.name))) continue;

                        // skip if this special sprite is over the limit of 51.
                        if (specialSprite.RowInSpriteSheet == -1) continue;
                        
                        var previousRow = __instance.crop.rowInSpriteSheet.Value;

                        // If this season is the one this special sprite is for, set the crop sprite to the special sprite.
                        __instance.crop.rowInSpriteSheet.Value = Game1.currentSeason == specialSprite.Season
                            ? specialSprite.RowInSpriteSheet
                            : adjustment.OriginalRowInSpriteSheet;

                        // Update crop draw math if the texture (row) was changed.
                        if (__instance.crop.rowInSpriteSheet.Value != previousRow)
                            __instance.crop.updateDrawMath(tileLocation);
                    }
                }
            }
        }
        
        /// <summary> Patch for the Crop.newDay method. </summary>
        public static void CropNewDay(Crop __instance, int state, int fertilizer, int xTile, int yTile,
            GameLocation environment)
        {
            // Return if this crop is still growing,
            // its hoe dirt is not watered or this crop is fiber.
            if ((!__instance.fullyGrown && (__instance.dayOfCurrentPhase != 0 && __instance.currentPhase != 5)) ||
                state != 1 || __instance.indexOfHarvest == 771) return;

            foreach (var contentPack in ModEntry.ContentPackManager.ContentPacks)
            {
                foreach (var adjustment in contentPack.CropAdjustments)
                {
                    // skip if this crop is not the desired one.
                    if (__instance.indexOfHarvest != adjustment.CropProduceItemId) continue;

                    ModEntry.ModMonitor.Log($"crop: {adjustment.CropProduceName}, regrowAfterHarvest: {__instance.regrowAfterHarvest}, currentPhase: {__instance.currentPhase}, dayOfCurrentPhase: {__instance.dayOfCurrentPhase}, fullyGrown: {__instance.fullyGrown}, phaseDays: {JsonConvert.SerializeObject(__instance.phaseDays)}", LogLevel.Info);

                    // return if the crop is planted in any of the locations where it grows all year round.
                    if (adjustment.GetLocationsToGrowAllYearRoundIn().Any(location => Utility.CompareTwoStringsCaseAndSpaceIndependently(location, environment.name))) return;
                    
                    // return if the crop is already in its produce season.
                    if (adjustment.GetSeasonsToProduceIn().Any(season => Game1.currentSeason == season)) return;

                    // kill the crop if it's out of its growth seasons.
                    if (adjustment.GetSeasonsToGrowIn().All(season => Game1.currentSeason != season))
                    {
                        __instance.Kill();
                        return;
                    }

                    // If the crop is about to finish its regrowth period in any season other than the produce seasons,
                    // prevent it from producing.
                    if (__instance.currentPhase == 5 && __instance.dayOfCurrentPhase == 0)
                    {
                        __instance.dayOfCurrentPhase.Value = 1;
                        __instance.fullyGrown.Value = true;
                    }
                }
            }
        }
    }
}
