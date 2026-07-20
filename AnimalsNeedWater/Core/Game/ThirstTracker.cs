using System;
using System.Collections.Generic;
using System.Linq;
using AnimalsNeedWater.Core.Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using Object = StardewValley.Object;

namespace AnimalsNeedWater.Core.Game;

public class ThirstTracker
{
    public List<ThirstyAnimalInfo> AnimalsLeftThirstyYesterday = new List<ThirstyAnimalInfo>();

    
    
    public bool WasAnimalLeftThirstyYesterday(FarmAnimal animal)
    {
        return AnimalsLeftThirstyYesterday.Any(info => info.Id == animal.myID.Value);
    }
    
    public void FindThirstyAnimals()
    { 
        // skip on farmhands to avoid double-deducting friendship points
        // host syncs this state with farmhands on day start
        if (!Context.IsMainPlayer) return;
        
        AnimalsLeftThirstyYesterday = new List<ThirstyAnimalInfo>();

        foreach (var locationGroup in ModEntry.BuildingTracker.AnimalBuildingGroups)
        {
            GameLocation parentLocation = locationGroup.Key;
            List<Building> buildingsInLocation = locationGroup.ToList();

            // Look for all animals inside buildings and check whether their troughs are watered
            // OR the building has a full Water Bowl inside it.
            foreach (Building building in buildingsInLocation)
            {
                // check if there are any full water bowls inside
                var hasFullWaterBowlObject = false;
                var buildingObjects = building.GetIndoors().Objects.Values;
                foreach (Object @object in buildingObjects)
                {
                    if (!@object.HasTypeId("(BC)") || @object.ItemId != ModConstants.WaterBowlItemId) 
                        continue;
                    if (@object.modData.ContainsKey(ModConstants.WaterBowlItemModDataIsFullField)
                        && @object.modData[ModConstants.WaterBowlItemModDataIsFullField] == "true")
                    {
                        hasFullWaterBowlObject = true;
                    }
                }
                if (hasFullWaterBowlObject)
                    continue;
                
                if (ModEntry.TroughManager.IsWatered(building))
                    continue;
                
                foreach (var animal in ((AnimalHouse) building.indoors.Value).animals.Values
                    .Where(animal =>
                        !ModEntry.Data.IsAnimalFull(animal) 
                        && AnimalsLeftThirstyYesterday.All(info => info.Id != animal.myID.Value)))
                {
                    animal.friendshipTowardFarmer.Value -= Math.Abs(ModEntry.Config.NegativeFriendshipPointsForNotWateredTrough);
                    AnimalsLeftThirstyYesterday.Add(new ThirstyAnimalInfo()
                    {
                        Id = animal.myID.Value, 
                        DisplayName = animal.displayName,
                        IsMale = animal.isMale(),
                    });
                }
            }

            // Check for animals outside their buildings as well.
            foreach (var animal in parentLocation.animals.Values)
            {
                if (animal.home != null &&
                    (ModEntry.TroughManager.IsWatered(animal.home) ||
                     ModEntry.Data.IsAnimalFull(animal)) &&
                    animal.home.animalDoorOpen.Value) continue;
            
                if (AnimalsLeftThirstyYesterday.Any(info => info.Id == animal.myID.Value)) continue;
                
                animal.friendshipTowardFarmer.Value -= Math.Abs(ModEntry.Config.NegativeFriendshipPointsForNotWateredTrough);
                AnimalsLeftThirstyYesterday.Add(new ThirstyAnimalInfo()
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
    
    public void ShowLeftThirstyMessage()
    {
        if (AnimalsLeftThirstyYesterday.Count == 0)
            return;
        
        string i18Key;
        switch (AnimalsLeftThirstyYesterday.Count)
        {
            case 1 when ModEntry.ModHelper.ModRegistry.IsLoaded("Paritee.GenderNeutralFarmAnimals"):
                i18Key = "AnimalsLeftWithoutWaterYesterday.globalMessage.oneAnimal_UnknownGender";
                break;
            case 1 when AnimalsLeftThirstyYesterday[0].IsMale:
                i18Key = "AnimalsLeftWithoutWaterYesterday.globalMessage.oneAnimal_Male";
                break;
            case 1 when !AnimalsLeftThirstyYesterday[0].IsMale:
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
        
        var firstAnimalName = AnimalsLeftThirstyYesterday[0].DisplayName;
        var secondAnimalName = AnimalsLeftThirstyYesterday.Count > 1 ? AnimalsLeftThirstyYesterday[1].DisplayName : "";
        var thirdAnimalName = AnimalsLeftThirstyYesterday.Count > 2 ? AnimalsLeftThirstyYesterday[2].DisplayName : "";
        Game1.showGlobalMessage(ModEntry.ModHelper.Translation.Get(
            i18Key,
            new
            {
                firstAnimalName,
                secondAnimalName,
                thirdAnimalName,
                totalAmountExcludingFirstThree = AnimalsLeftThirstyYesterday.Count - 3
            }));
    }
}