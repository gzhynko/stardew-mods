using System.Collections.Generic;
using System.Linq;
using AnimalsNeedWater.Core.Models;
using StardewModdingAPI;
using StardewValley;

namespace AnimalsNeedWater.Core.Game;

public class ThirstTracker
{
    public bool WasAnimalLeftThirstyYesterday(FarmAnimal animal)
    {
        return ModEntry.Data.AnimalsLeftThirstyYesterday.Any(info => info.Id == animal.myID.Value);
    }

    public void FindThirstyAnimals()
    {
        // skip on farmhands to avoid double-deducting friendship points
        // host syncs this state with farmhands on day start
        if (!Context.IsMainPlayer) return;

        ModEntry.Data.AnimalsLeftThirstyYesterday = new List<ThirstyAnimalInfo>();
        var added = new HashSet<long>();

        foreach (var locationGroup in ModEntry.BuildingTracker.AnimalBuildingGroups)
        {
            // look for animals in the location itself (outside) and inside buildings
            foreach (var animal in locationGroup.Key.getAllFarmAnimals())
            {
                if (!added.Add(animal.myID.Value)) continue;

                var settings = ModEntry.AnimalSettings.For(animal);
                if (!settings.NeedsWater) continue; // opts out of the whole system

                // had access today -> reset the clock, never thirsty
                if (HasWaterAccess(animal))
                {
                    SetLastWateredDay(animal);
                    continue;
                }

                // first ever evaluation -> persist a grace day, no penalty this cycle
                if (ReadLastWateredDay(animal) == int.MinValue)
                {
                    SetLastWateredDay(animal);
                    continue;
                }

                if (!IntervalDue(animal)) continue;
                
                // no access and interval elapsed -> thirsty

                ModEntry.AnimalFriendship.RemoveThirstyPenaltyFriendship(animal);

                ModEntry.Data.AnimalsLeftThirstyYesterday.Add(new ThirstyAnimalInfo()
                {
                    Id = animal.myID.Value,
                    DisplayName = animal.displayName,
                    IsMale = animal.isMale(),
                });
            }
        }
    }

    public void ResetFullAnimals()
    {
        ModEntry.Data.ResetFullAnimals();
    }

    public void AddFullAnimal(long animalId)
    {
        ModEntry.Data.FullAnimals.Add(animalId);
    }

    public int ReadLastWateredDay(FarmAnimal animal)
    {
        if (animal.modData.TryGetValue(ModConstants.ModDataLastWateredDayKey, out var raw)
            && int.TryParse(raw, out var day))
            return day;
        return int.MinValue; // unseeded
    }

    private void SetLastWateredDay(FarmAnimal animal)
        => animal.modData[ModConstants.ModDataLastWateredDayKey] = Game1.Date.TotalDays.ToString();

    public bool HasWaterAccess(FarmAnimal animal)
    {
        if (ModEntry.Data.IsAnimalFull(animal)) return true; // drank outside today
        if (animal.home == null || !ModEntry.BuildingWaterManager.IsWatered(animal.home)) return false;
        bool insideHome = animal.currentLocation.Equals(animal.home.GetIndoors());
        return insideHome || animal.home.animalDoorOpen.Value; // if outside  -> needs an open door
    }

    public bool IntervalDue(FarmAnimal animal)
    {
        int last = ReadLastWateredDay(animal);
        if (last == int.MinValue) return false;
        return (Game1.Date.TotalDays - last) >= ModEntry.AnimalSettings.For(animal).WaterIntervalDays;
    }

    public bool IsAnimalThirsty(FarmAnimal animal)
    {
        if (!ModEntry.AnimalSettings.For(animal).NeedsWater) return false;
        return !HasWaterAccess(animal) && IntervalDue(animal);
    }

    public void ShowLeftThirstyMessage()
    {
        var animalsLeftThirsty = ModEntry.Data.AnimalsLeftThirstyYesterday;
        if (animalsLeftThirsty.Count == 0)
            return;

        string i18Key;
        switch (animalsLeftThirsty.Count)
        {
            case 1 when ModEntry.ModHelper.ModRegistry.IsLoaded("Paritee.GenderNeutralFarmAnimals"):
                i18Key = "AnimalsLeftWithoutWaterYesterday.globalMessage.oneAnimal_UnknownGender";
                break;
            case 1 when animalsLeftThirsty[0].IsMale:
                i18Key = "AnimalsLeftWithoutWaterYesterday.globalMessage.oneAnimal_Male";
                break;
            case 1 when !animalsLeftThirsty[0].IsMale:
                i18Key = "AnimalsLeftWithoutWaterYesterday.globalMessage.oneAnimal_Female";
                break;
            case 2:
                i18Key = "AnimalsLeftWithoutWaterYesterday.globalMessage.twoAnimals";
                break;
            case 3:
                i18Key = "AnimalsLeftWithoutWaterYesterday.globalMessage.threeAnimals";
                break;
            default:
                i18Key = "AnimalsLeftWithoutWaterYesterday.globalMessage.multipleAnimals";
                break;
        }

        var firstAnimalName = animalsLeftThirsty[0].DisplayName;
        var secondAnimalName = animalsLeftThirsty.Count > 1 ? animalsLeftThirsty[1].DisplayName : "";
        var thirdAnimalName = animalsLeftThirsty.Count > 2 ? animalsLeftThirsty[2].DisplayName : "";
        Game1.showGlobalMessage(ModEntry.ModHelper.Translation.Get(
            i18Key,
            new
            {
                firstAnimalName,
                secondAnimalName,
                thirdAnimalName,
                totalAmountExcludingFirstThree = animalsLeftThirsty.Count - 3
            }));
    }
}