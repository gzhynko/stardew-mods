using System;
using StardewValley;

namespace AnimalsNeedWater.Core.Game;

public class AnimalFriendship
{
    public void AddDailyWaterFriendship(FarmAnimal animal)
    {
        var settings = ModEntry.AnimalSettings.For(animal);
        var amount = Math.Abs(settings.DailyWaterFriendship ?? ModEntry.Config.FriendshipPointsForWateredTrough);
        animal.friendshipTowardFarmer.Value += amount;
        animal.friendshipTowardFarmer.Value = Math.Clamp(animal.friendshipTowardFarmer.Value, 0, 1000);
    }

    public void AddWitnessedWaterFriendship(FarmAnimal animal)
    {
        var settings = ModEntry.AnimalSettings.For(animal);
        var amount = Math.Abs(settings.WitnessedWaterFriendship ?? ModEntry.Config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding);
        animal.friendshipTowardFarmer.Value += amount;
        animal.friendshipTowardFarmer.Value = Math.Clamp(animal.friendshipTowardFarmer.Value, 0, 1000);
    }
    
    public void RemoveThirstyPenaltyFriendship(FarmAnimal animal)
    {
        var settings = ModEntry.AnimalSettings.For(animal);
        var amount = Math.Abs(settings.ThirstyFriendshipPenalty ?? ModEntry.Config.NegativeFriendshipPointsForNotWateredTrough);
        animal.friendshipTowardFarmer.Value -= amount;
        animal.friendshipTowardFarmer.Value = Math.Clamp(animal.friendshipTowardFarmer.Value, 0, 1000);
    }
}