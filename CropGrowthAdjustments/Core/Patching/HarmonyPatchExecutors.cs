using System;
using System.Linq;
using CropGrowthAdjustments.Core.Game;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace CropGrowthAdjustments.Core.Patching;

internal static class HarmonyPatchExecutors
{
    /// <summary> Postfix for the HoeDirt.plant method </summary>
    public static void HoeDirtPlant(HoeDirt __instance, ref bool __result)
    {
        if (!__result) return;

        // change sprites to special if needed
        CropSpriteManager.ApplySpritesFor(__instance);
    }
    
    /// <summary>
    /// Prefix for the Crop.newDay method.
    ///
    /// freezes an affected crop one step before it becomes harvestable while outside its produce seasons;
    /// the freeze lifts on the first night of a produce season, making the crop harvestable that morning.
    /// Crop survival/death is handled in the per-location season check that kills crops outside produce+grow seasons
    /// </summary>
    public static bool CropNewDay(Crop __instance, int state)
    {
        var location = __instance.currentLocation;
        if (location == null) return true;

        // nothing grows this day -> run original newDay
        if (state != 1 && (__instance.GetData()?.NeedsWatering ?? true)) return true;

        // greenhouse -> run original newDay
        if (location.SeedsIgnoreSeasonsHere()) return true;

        var season = location.GetSeason();
        var cropHarvestId = Utility.NormalizeItemId(__instance.indexOfHarvest.Value ?? "");
        foreach (var contentPack in ModEntry.ContentPackLoader.ContentPacks)
        {
            foreach (var adjustment in contentPack.CropAdjustments)
            {
                // skip if this crop is not the desired one.
                if (cropHarvestId != Utility.NormalizeItemId(adjustment.CropProduceItemId)) continue;
                // vanilla behavior in locations the content pack excluded
                if (Utility.IsInAnyOfSpecifiedLocations(adjustment.ParsedLocationsWithDefaultBehavior, location)) return true;
                // in a hibernate season: pause growth entirely
                if (adjustment.ParsedSeasonsToHibernateIn.Contains(season)) return false;
                // in a produce season: grow and produce normally
                if (adjustment.ParsedSeasonsToProduceIn.Contains(season)) return true;
                // grow-only: freeze the crop on the day it would become harvestable
                if (adjustment.ParsedSeasonsToGrowIn.Contains(season)) return !WouldBecomeHarvestable(__instance);
                
                // out of all seasons: let the game kill it
                return true;
            }
        }

        return true;
    }
    
    /// <summary> returns whether running Crop.newDay now would make the crop harvestable </summary>
    private static bool WouldBecomeHarvestable(Crop crop)
    {
        var phaseDays = crop.phaseDays;
        var phaseCount = phaseDays.Count;
        if (phaseCount == 0) return false;

        // regrowing crop that already produced -> harvestable again when its countdown reaches zero
        if (crop.fullyGrown.Value)
        {
            return crop.dayOfCurrentPhase.Value > 0 && crop.dayOfCurrentPhase.Value <= 1;
        }

        // already in the final phase -> whether harvestable or not, there is nothing left to hold back
        if (crop.currentPhase.Value >= phaseCount - 1) return false;

        // simulate the vanilla growth step
        var phase = crop.currentPhase.Value;
        var day = Math.Min(crop.dayOfCurrentPhase.Value + 1, phaseDays[Math.Min(phaseCount - 1, phase)]);
        if (day >= phaseDays[Math.Min(phaseCount - 1, phase)] && phase < phaseCount - 1)
        {
            phase++;
        }
        while (phase < phaseCount - 1 && phaseDays[phase] <= 0)
        {
            phase++;
        }

        return phase >= phaseCount - 1;
    }
}