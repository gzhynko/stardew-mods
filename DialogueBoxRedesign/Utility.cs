using StardewValley;
using StardewValley.Menus;

namespace DialogueBoxRedesign;

public static class Utility
{
    public static bool ShouldPortraitShake(DialogueBox dialogueBox, NPC speaker, Dialogue dialogue)
    {
        if (dialogueBox.newPortaitShakeTimer > 0)
            return true;
        return dialogue.speaker.GetData()?.ShakePortraits?.Contains(dialogue.getPortraitIndex()) == true;
    }
}