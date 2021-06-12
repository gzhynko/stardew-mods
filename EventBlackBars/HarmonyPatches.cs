using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace EventBlackBars
{
    public class HarmonyPatches
    {
        /// <summary> Patch for the Game1.eventFinished method. </summary>
        public static void EventEnd()
        {
            ModEntry.Instance.StartMovingBars(false);
        }
        
        /// <summary> Patch for the GameLocation.startEvent method. </summary>
        public static void EventStart(Event evt)
        {
            if (evt.isFestival || evt.isWedding) return;
            
            ModEntry.Instance.StartMovingBars(true);
        }
    }
}