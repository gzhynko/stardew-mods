using System;
using AnimalsNeedWater.Core.Models;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;

namespace AnimalsNeedWater.Core.Multiplayer;

public class MessageBridge
{
    public void SendTroughWateredMessage(string building)
    {
        SendMessageToSelf(new TroughWateredMessage(building), "TroughWateredMessage");
    }
    
    public void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
    {
        if (e.FromModID != ModEntry.Manifest.UniqueID || e.Type != "TroughWateredMessage") return;
        
        TroughWateredMessage message = e.ReadAs<TroughWateredMessage>();
        
        string locationName = message.BuildingUniqueName;
        var location = Game1.getLocationFromName(locationName);
        if (location == null)
            return; 
        
        var building = location.ParentBuilding;
            
        ModEntry.TroughManager.MarkWatered(building);
        
        if (string.Equals(Game1.currentLocation.NameOrUniqueName, message.BuildingUniqueName,
                StringComparison.OrdinalIgnoreCase))
        {
            ModEntry.TroughVisuals.ApplyTroughTiles(building);
        }
    }

    private void SendMessageToSelf(object message, string messageType)
    {
        ModEntry.ModHelper.Multiplayer.SendMessage(message, messageType, new[] { ModEntry.Manifest.UniqueID });
    }
}