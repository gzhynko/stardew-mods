using StardewModdingAPI;
using StardewValley;

//ReSharper disable InconsistentNaming

namespace EventBlackBars
{
    public class HarmonyPatches
    {
        /// <summary> Patch for the Event.exitEvent method. </summary>
        public static void EventEnd(Event __instance)
        {
            if (__instance.isFestival || __instance.isWedding) return;
            
            ModEntry.Instance.StartMovingBars(Direction.MoveOut);
        }
        
        /// <summary> Patch for the GameLocation.startEvent method. </summary>
        public static void EventStart(Event evt)
        {
            if (evt.isFestival || evt.isWedding) return;
            
            ModEntry.Instance.StartMovingBars(Direction.MoveIn);
        }
    }
}