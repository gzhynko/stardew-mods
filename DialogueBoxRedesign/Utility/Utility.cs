using StardewValley;
using StardewValley.Menus;

namespace DialogueBoxRedesign.Utility
{
    public static class Utility
    {
        public static bool ShouldPortraitShake(DialogueBox dialogueBox, Dialogue dialogue)
        {
            var portraitIndex = dialogue.getPortraitIndex();
            
            return dialogue.speaker.Name.Equals("Pam") && portraitIndex == 3 ||
                   dialogue.speaker.Name.Equals("Abigail") && portraitIndex == 7 ||
                   (dialogue.speaker.Name.Equals("Haley") && portraitIndex == 5 ||
                    dialogue.speaker.Name.Equals("Maru") && portraitIndex == 9) || dialogueBox.newPortaitShakeTimer > 0;
        }
    }
}