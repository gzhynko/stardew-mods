using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace AnimalsNeedWater.Core.Game;

public class AnimalEmotes
{
    private const int LoveEmoteId = 20;
    private const int SadEmoteId = 28;
    private const int HappyEmoteId = 32;

    private const int ThirstyRollIntervalTicks = 8;
    private const double ThirstyEmoteChancePerRoll = 0.0004 * ThirstyRollIntervalTicks;
    
    // show thirsty emotes
    public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(ThirstyRollIntervalTicks)) return;
        if (!Context.IsWorldReady || Game1.eventUp) return;
        if (!ModEntry.Config.ShowThirstyEmotes) return;
        if (Game1.timeOfDay < Math.Clamp(ModEntry.Config.ThirstyEmoteStartTime, 600, 2600)) return;

        var location = Game1.currentLocation;
        if (location == null || location.animals.Length == 0) return;

        foreach (var animal in location.animals.Values)
        {
            if (animal.IsEmoting || animal.home == null) continue;
            if (Game1.random.NextDouble() >= ThirstyEmoteChancePerRoll) continue;
            if (ModEntry.Data.IsAnimalFull(animal)) continue;
            if (ModEntry.TroughManager.IsWatered(animal.home)) continue;

            EmoteThirsty(animal);
        }
    }

    public void EmoteBonus(FarmAnimal animal)
    {
        animal.doEmote(LoveEmoteId);
    }
    
    public void EmoteDrankOutside(FarmAnimal animal)
    {
        animal.doEmote(HappyEmoteId);
    }

    private void EmoteThirsty(FarmAnimal animal)
    {
        animal.doEmote(SadEmoteId);
    }
}