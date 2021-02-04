using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace EventBlackBars
{
    public class HarmonyPatches
    {
        public static void EventEnd()
        {
            ModEntry.Instance.StartMovingBars(false);
            ModEntry.ModMonitor.Log("evtEnd", LogLevel.Alert);
        }
        
        public static void EventStart(Event evt)
        {
            if (evt.isFestival) return;
            
            ModEntry.Instance.StartMovingBars(true);
            ModEntry.ModMonitor.Log("evtStart", LogLevel.Alert);
        }
    }
}