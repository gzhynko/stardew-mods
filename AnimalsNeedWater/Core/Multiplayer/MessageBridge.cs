using System;
using System.Collections.Generic;
using AnimalsNeedWater.Core.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;

namespace AnimalsNeedWater.Core.Multiplayer;

public class MessageBridge
{
    private const string TroughWateredMessageType = "TroughWateredMessage";
    private const string StateSyncMessageType = "StateSyncMessage";
    private const string RequestStateMessageType = "RequestStateMessage";
    private const string ThirstyAnimalsMessageType = "ThirstyAnimalsMessage";
    private const string AddFullAnimalMessageType = "AddFullAnimalMessage";
    
    public void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
    {
        if (e.FromModID != ModEntry.Manifest.UniqueID) return;

        switch (e.Type)
        {
            case TroughWateredMessageType:
                TroughWateredMessage troughWateredMessage = e.ReadAs<TroughWateredMessage>();
                
                string locationName = troughWateredMessage.BuildingUniqueName;
                var location = Game1.getLocationFromName(locationName);

                var targetBuilding = location?.ParentBuilding;
                if (targetBuilding == null)
                    return;
                
                ModEntry.TroughManager.MarkWatered(targetBuilding);
        
                var currentLocationName = Game1.currentLocation?.NameOrUniqueName;
                if (currentLocationName == null)
                    return;
                
                // immediately sync visual state if in same building
                if (string.Equals(currentLocationName, locationName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    ModEntry.TroughVisuals.ApplyTroughTiles(targetBuilding);
                }
                
                break;
            
            case StateSyncMessageType:
                // only farmhands act on this message (host -> farmhand)
                if (Context.IsMainPlayer) return;
                
                StateSyncMessage stateSyncMessage = e.ReadAs<StateSyncMessage>();
                
                ModEntry.Data.BuildingsWithWateredTrough =
                    new HashSet<string>(stateSyncMessage.BuildingsWithWateredTrough, StringComparer.OrdinalIgnoreCase);
                ModEntry.Data.BuildingsWithBonusAwardedToday =
                    new HashSet<string>(stateSyncMessage.BuildingsWithBonusAwardedToday, StringComparer.OrdinalIgnoreCase);
                ModEntry.Data.FullAnimals = stateSyncMessage.FullAnimals;

                foreach (var building in ModEntry.BuildingTracker.AnimalBuildings)
                {
                    ModEntry.TroughVisuals.ReapplyVisuals(building);
                }
                
                break;
            
            case ThirstyAnimalsMessageType:
                if (Context.IsMainPlayer) return;
                
                ThirstyAnimalsMessage thirstyAnimalsMessage = e.ReadAs<ThirstyAnimalsMessage>();

                ModEntry.ThirstTracker.AnimalsLeftThirstyYesterday = thirstyAnimalsMessage.ThirstyAnimals;

                if (ModEntry.Config.ShowAnimalsLeftThirstyMessage)
                {
                    ModEntry.ThirstTracker.ShowLeftThirstyMessage();
                }

                break;
            
            case AddFullAnimalMessageType:
                if (Context.IsMainPlayer) return;
                
                AddFullAnimalMessage addFullAnimalMessage = e.ReadAs<AddFullAnimalMessage>();
                ModEntry.ThirstTracker.AddFullAnimal(addFullAnimalMessage.AnimalId);
                
                break;
            
            case RequestStateMessageType:
                if (!Context.IsMainPlayer) return;
                
                RequestStateMessage requestStateMessage  = e.ReadAs<RequestStateMessage>();
                if (requestStateMessage.RequesterId != e.FromPlayerID) return;
                
                SendStateSyncMessage(ModEntry.Data.BuildingsWithWateredTrough, ModEntry.Data.BuildingsWithBonusAwardedToday, ModEntry.Data.FullAnimals, requestStateMessage.RequesterId);
                
                break;
        }
    }
    
    public void SendTroughWateredMessage(Building building)
    {
        SendMessageToOwnMod(new TroughWateredMessage(building.GetIndoors().NameOrUniqueName), TroughWateredMessageType);
    }
    
    public void SendStateSyncMessage(HashSet<string> buildings, HashSet<string> bonuses, HashSet<long> fullAnimals, long? receiverId = null)
    {
        var playerIds = receiverId != null ? new long[] { (long)receiverId } : null;
        SendMessageToOwnMod(new StateSyncMessage(buildings, bonuses, fullAnimals), StateSyncMessageType, playerIds);
    }

    public void SendRequestStateMessage(long requesterId)
    {
        var hostId = Game1.MasterPlayer?.UniqueMultiplayerID;
        var playerIds = hostId != null ? new long[] { (long)hostId } : null;
        SendMessageToOwnMod(new RequestStateMessage(requesterId), RequestStateMessageType, playerIds);
    }
    
    public void SendThirstyAnimalsMessage(List<ThirstyAnimalInfo> animals)
    {
        SendMessageToOwnMod(new ThirstyAnimalsMessage(animals), ThirstyAnimalsMessageType);
    }
    
    public void SendAddFullAnimalMessage(FarmAnimal animal)
    {
        SendMessageToOwnMod(new AddFullAnimalMessage(animal.myID.Value), AddFullAnimalMessageType);
    }

    private void SendMessageToOwnMod(object message, string messageType, long[]? playerIds = null)
    {
        ModEntry.ModHelper.Multiplayer.SendMessage(message, messageType, new[] { ModEntry.Manifest.UniqueID }, playerIds);
    }
}