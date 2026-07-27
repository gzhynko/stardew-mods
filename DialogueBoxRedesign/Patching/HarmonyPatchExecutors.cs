using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace DialogueBoxRedesign.Patching;

public static class HarmonyPatchExecutors
{
    private const int WidthOfPortraitArea = 444;
    private const int BoxHeight = 250;
    private const int BottomMargin = 64; // panel = BoxHeight + BottomMargin tall

    private static bool RedesignsQuestion(DialogueBox box) =>
        box.isQuestion && ModEntry.Config.ChangeQuestionDesign;

    private static bool RedesignsDialogue(DialogueBox box) =>
        !box.isQuestion && box.isPortraitBox() && ModEntry.Config.ChangeDialogueDesign;

    public static bool DrawPortrait(DialogueBox dialogueBox, SpriteBatch spriteBatch)
    {
        if (!ModEntry.Config.ChangeDialogueDesign) return true;

        if (dialogueBox.width < 642) return true;

        var npcSpeaker = dialogueBox.characterDialogue.speaker;
        if (!Game1.IsMasterGame && !npcSpeaker.EventActor)
        {
            var currentLocation = npcSpeaker.currentLocation;
            if (currentLocation?.IsActiveLocation() != true)
            {
                var characterFromName = Game1.getCharacterFromName(npcSpeaker.Name);
                if (characterFromName != null && characterFromName.currentLocation.IsActiveLocation())
                    npcSpeaker = characterFromName;
            }
        }

        int xPositionOfPortraitArea;
        if (ModEntry.Config.ShowPortraitOnTheLeft)
        {
            xPositionOfPortraitArea = dialogueBox.x;
        }
        else
        {
            xPositionOfPortraitArea = dialogueBox.x + dialogueBox.width - WidthOfPortraitArea;
        }

        var portraitBoxX = xPositionOfPortraitArea + 76;
        var portraitBoxY = dialogueBox.y + dialogueBox.height / 2 - 148 - 36;
        var portraitScale = 4f;

        var portraitTexture = dialogueBox.characterDialogue.overridePortrait ??
                              npcSpeaker.Portrait;
        
        var portCellSize = portraitTexture.Width / 2;
        var index = dialogueBox.characterDialogue.getPortraitIndex();
        var portraitSource = new Rectangle(index % 2 * portCellSize, index / 2 * portCellSize, portCellSize, portCellSize);
        
        /* HD Portraits Compat */
        if (ModEntry.HdPortraitsApi != null)
        {
            var data = ModEntry.HdPortraitsApi.GetTextureAndRegion(
                npcSpeaker,
                dialogueBox.characterDialogue.getPortraitIndex(),
                Game1.currentGameTime.ElapsedGameTime.Milliseconds
            ); //no need to force reset, HD portraits auto-resets after dialogue box close

            if (dialogueBox.characterDialogue.overridePortrait == null)
            {
                portraitTexture = data.Item2;
                portraitSource = data.Item1;
            }
        }

        if (!portraitTexture.Bounds.Contains(portraitSource)) portraitSource = new Rectangle(0, 0, portCellSize, portCellSize);

        var xOffset = Utility.ShouldPortraitShake(dialogueBox, npcSpeaker, dialogueBox.characterDialogue)
            ? Game1.random.Next(-1, 2)
            : 0;

        /* Portrait */
        spriteBatch.Draw(portraitTexture,
            new Rectangle(portraitBoxX + 16 + xOffset, Game1.uiViewport.Height - 256, 256, 256),
            portraitSource, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.88f);

        var speakerNameX = xPositionOfPortraitArea + WidthOfPortraitArea / 2;
        var speakerNameY = portraitBoxY + 50;

        /* Speaker name */
        if (ModEntry.Config.ShowSpeakerName)
        {
            DrawTextWithShadowOrOutline(spriteBatch, npcSpeaker.getName(), speakerNameX, speakerNameY,
                0, 0, Color.White, Color.Black, horCentered: true);
        }

        /* Friendship jewel */
        if (dialogueBox.shouldDrawFriendshipJewel() && ModEntry.Config.ShowFriendshipJewel)
        {
            var jewelBottomOffset = 80;
            var jewelLeftOffset = 40;

            dialogueBox.friendshipJewel.Y = Game1.uiViewport.Height - jewelBottomOffset;
            dialogueBox.friendshipJewel.X = (int)(portraitBoxX + 64 * portraitScale + jewelLeftOffset);

            spriteBatch.Draw(Game1.mouseCursors,
                new Vector2(dialogueBox.friendshipJewel.X, dialogueBox.friendshipJewel.Y),
                Game1.player.getFriendshipHeartLevelForNPC(npcSpeaker.Name) >= 10
                    ? new Rectangle(269, 494, 11, 11)
                    : new Rectangle(
                        Math.Max(140,
                            140 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 / 250.0) *
                            11),
                        Math.Max(532,
                            532 + Game1.player.getFriendshipHeartLevelForNPC(npcSpeaker
                                .Name) / 2 * 11), 11, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None,
                0.88f);

            // friendship jewel tooltip
            var hoverText = ModEntry.ModHelper.Reflection.GetField<string>(dialogueBox, "hoverText").GetValue();
            if (hoverText.Length > 0 && ModEntry.Config.ShowFriendshipJewel)
            {
                SpriteText.drawStringWithScrollBackground(spriteBatch, hoverText,
                    dialogueBox.friendshipJewel.Center.X - SpriteText.getWidthOfString(hoverText) / 2,
                    dialogueBox.friendshipJewel.Y - 64);
            }
        }

        return false; // dont run original method
    }

    public static bool DrawBox(DialogueBox dialogueBox, SpriteBatch spriteBatch, int xPos, int yPos, int boxWidth,
        int boxHeight)
    {
        if (!dialogueBox.transitionInitialized) return true;
        if (!RedesignsQuestion(dialogueBox) && !RedesignsDialogue(dialogueBox)) return true;

        var viewportHeight = Game1.uiViewport.Height;

        if (dialogueBox.transitioning)
        {
            var progress = dialogueBox.transitioningBigger
                ? 1f - MathHelper.Clamp((xPos - dialogueBox.x) / (dialogueBox.width / 2f), 0f, 1f)
                : MathHelper.Clamp(boxWidth / (float)dialogueBox.width, 0f, 1f);

            var fullTop = dialogueBox.isQuestion
                ? dialogueBox.y - (dialogueBox.heightForQuestions - dialogueBox.height)
                : viewportHeight - BoxHeight - BottomMargin;
            fullTop -= ModEntry.Config.BoxBackgroundGradientLead;

            var top = viewportHeight - (int)((viewportHeight - fullTop) * progress);

            DrawBackdrop(spriteBatch, top, ModEntry.Config.BoxBackgroundOpacity * progress);
            return false;
        }

        if (dialogueBox.isQuestion)
        {
            // ypos is question top here
            var top = yPos - ModEntry.Config.BoxBackgroundGradientLead;
            DrawBackdrop(spriteBatch, top, ModEntry.Config.BoxBackgroundOpacity);
            return false;
        }

        dialogueBox.height = BoxHeight;
        dialogueBox.y = viewportHeight - dialogueBox.height - BottomMargin;
        var gradientTop = dialogueBox.y - ModEntry.Config.BoxBackgroundGradientLead;
        DrawBackdrop(spriteBatch, gradientTop, ModEntry.Config.BoxBackgroundOpacity);

        return false;
    }

    private static void DrawQuestion(DialogueBox dialogueBox, SpriteBatch spriteBatch)
    {
        var question = dialogueBox.getCurrentString();
        var questionTop = dialogueBox.y - (dialogueBox.heightForQuestions - dialogueBox.height);

        dialogueBox.drawBox(spriteBatch, dialogueBox.x, questionTop, dialogueBox.width,
            dialogueBox.heightForQuestions);

        // question icon
        spriteBatch.Draw(Game1.mouseCursors_1_6,
            new Vector2((float)(dialogueBox.x + dialogueBox.width - 7 * 4), (float)(questionTop + 12)),
            new Rectangle?(new Rectangle(
                470 + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 900 / 150 * 7, 447, 7, 12)),
            Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);

        // question text
        DrawTextWithShadowOrOutline(spriteBatch, question, dialogueBox.x + 8, questionTop + 12,
            dialogueBox.characterIndexInDialogue, dialogueBox.width - 16, Color.White, Color.Black);

        if (dialogueBox.characterIndexInDialogue >= question.Length - 1)
        {
            // populate response options
            var responseY = questionTop + SpriteText.getHeightOfString(question, dialogueBox.width - 16) + 48;
            for (var i = 0; i < dialogueBox.responses.Length; i++)
            {
                var responseHeight =
                    SpriteText.getHeightOfString(dialogueBox.responses[i].responseText, dialogueBox.width);
                if (i == dialogueBox.selectedResponse)
                    IClickableMenu.drawTextureBox(spriteBatch, Game1.mouseCursors, new Rectangle(375, 357, 3, 3),
                        dialogueBox.x + 4, responseY - 8, dialogueBox.width - 8, responseHeight + 16,
                        Color.White, 4f, drawShadow: false);
                
                DrawTextWithShadowOrOutline(spriteBatch, dialogueBox.responses[i].responseText, dialogueBox.x + 8,
                    responseY,
                    999999, dialogueBox.width - 16, Color.White, Color.Black,
                    alpha: dialogueBox.selectedResponse == i ? 1f : 0.6f);

                responseY += responseHeight + 16;
            }
        }

        var aboveDialogueImg = dialogueBox.aboveDialogueImage;
        if (aboveDialogueImg != null)
            StardewValley.Utility.drawWithShadow(spriteBatch, aboveDialogueImg.texture,
                new Vector2(
                    dialogueBox.x + dialogueBox.width / 2f -
                    aboveDialogueImg.sourceRect.Width / 2f * aboveDialogueImg.scale,
                    dialogueBox.y - 64 - aboveDialogueImg.sourceRect.Height * aboveDialogueImg.scale),
                aboveDialogueImg.sourceRect, Color.White, 0f, Vector2.Zero, aboveDialogueImg.scale, flipped: false, 1f);
    }


    public static bool Draw(DialogueBox dialogueBox, SpriteBatch spriteBatch)
    {
        if (dialogueBox.width < 16 || dialogueBox.height < 16 || dialogueBox.transitioning)
            return true;
        
        if (RedesignsQuestion(dialogueBox))
        {
            DrawQuestion(dialogueBox, spriteBatch);
        }
        else if (RedesignsDialogue(dialogueBox))
        {
            var viewportWidth = Game1.uiViewport.Width;
            dialogueBox.width = viewportWidth > 1600 ? 1500 : 1200;
            dialogueBox.x = (viewportWidth - dialogueBox.width) / 2;
            
            dialogueBox.drawBox(spriteBatch, dialogueBox.x, dialogueBox.y, dialogueBox.width, dialogueBox.height);

            var currentText = dialogueBox.getCurrentString();
            int textX;
            if (ModEntry.Config.ShowPortraitOnTheLeft)
            {
                textX = dialogueBox.x + WidthOfPortraitArea;
            }
            else
            {
                textX = dialogueBox.x;
            }

            var textWidth = dialogueBox.width - WidthOfPortraitArea;
            var textY = dialogueBox.y + 58;

            DrawTextWithShadowOrOutline(spriteBatch, currentText, textX, textY,
                dialogueBox.characterIndexInDialogue, textWidth, Color.White, Color.Black);

            dialogueBox.drawPortrait(spriteBatch);
            
            if (dialogueBox.dialogueIcon != null && dialogueBox.characterIndexInDialogue >= currentText.Length - 1)
            {
                dialogueBox.dialogueIcon.Position = new Vector2(textX + textWidth, dialogueBox.dialogueIcon.Position.Y);
                dialogueBox.dialogueIcon.draw(spriteBatch, true);
            }
        }
        else
        {
            return true;
        }

        dialogueBox.drawMouse(spriteBatch);

        return false;
    }

    private static void DrawBackdrop(SpriteBatch spriteBatch, int top, float opacityMultiplier = 1f)
    {
        var viewport = Game1.uiViewport;
        spriteBatch.Draw(ModEntry.GradientSample, new Rectangle(0, top, viewport.Width, viewport.Height - top),
            Color.White * (ModEntry.Config.BoxBackgroundOpacity * opacityMultiplier));
    }

    private static readonly (int X, int Y)[] OutlineOffsets = [(-3, 0), (3, 0), (0, -3), (0, 3)];
    private static readonly (int X, int Y)[] ShadowOffsets = [(-3, 3)];

    private static void DrawTextWithShadowOrOutline(SpriteBatch spriteBatch, string text, int textX, int textY,
        int charPosition, int textWidth, Color textColor, Color outlineColor, float alpha = 1f,
        bool horCentered = false)
    {
        // outline or shadow
        var offsetsToDraw = ModEntry.Config.DrawTextOutline
            ? OutlineOffsets
            : ShadowOffsets;
        var offsetOpacity = ModEntry.Config.DrawTextOutline ? 0.6f : 0.8f;
        foreach (var (ox, oy) in offsetsToDraw)
        {
            if (horCentered)
            {
                SpriteText.drawStringHorizontallyCenteredAt(spriteBatch,
                    text, textX + ox, textY + oy, color: outlineColor * offsetOpacity);
            }
            else
            {
                SpriteText.drawString(
                    spriteBatch,
                    text,
                    textX + ox,
                    textY + oy,
                    charPosition,
                    textWidth,
                    color: outlineColor * offsetOpacity,
                    alpha: alpha);
            }
        }

        // actual text
        if (horCentered)
        {
            SpriteText.drawStringHorizontallyCenteredAt(spriteBatch,
                text, textX, textY, color: textColor);
        }
        else
        {
            SpriteText.drawString(
                spriteBatch,
                text,
                textX,
                textY,
                charPosition,
                textWidth,
                color: textColor,
                alpha: alpha);
        }
    }
}