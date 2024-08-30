using Common.Patching;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Constants = StardewElectricity.Utility.Constants;

namespace StardewElectricity.Patching;

public class GameMenuPatcher : BasePatcher
{
    public override void Apply(Harmony harmony, IMonitor modMonitor)
    {
        harmony.Patch(
            AccessTools.Method(typeof(GameMenu), nameof(GameMenu.getTabNumberFromName)),
            prefix: new HarmonyMethod(this.GetType(), nameof(GameMenuPatcher.GetTabNumberFromName_Prefix))
        );
        harmony.Patch(
            AccessTools.Method(typeof(GameMenu), nameof(GameMenu.draw), new []{ typeof(SpriteBatch) }),
            prefix: new HarmonyMethod(this.GetType(), nameof(GameMenuPatcher.Draw_Prefix))
        );
    }

    private static bool GetTabNumberFromName_Prefix(GameMenu __instance, ref int __result, string name)
    {
        if (name != Constants.GameMenuElectricityTabName) return true;
        
        __result = Constants.GameMenuElectricityTabIndex;
        return false;
    }

    private static bool Draw_Prefix(GameMenu __instance, SpriteBatch b)
    {
        if (__instance.invisible) return true;
        
        if (!Game1.options.showMenuBackground && !Game1.options.showClearBackgrounds)
          b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
        Game1.drawDialogueBox(__instance.xPositionOnScreen, __instance.yPositionOnScreen, __instance.pages[__instance.currentTab].width, __instance.pages[__instance.currentTab].height, false, true);
        b.End();
        b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
        foreach (ClickableComponent tab in __instance.tabs)
        {
          int num = -1;
          switch (tab.name)
          {
            case "animals":
              b.Draw(Game1.mouseCursors_1_6, new Vector2((float) tab.bounds.X, (float) (tab.bounds.Y + (__instance.currentTab == __instance.getTabNumberFromName(tab.name) ? 8 : 0))), new Rectangle?(new Rectangle(257, 246, 16, 16)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
              break;
            case "catalogue":
              num = 7;
              break;
            case "collections":
              num = 5;
              break;
            case "coop":
              num = 1;
              break;
            case "crafting":
              num = 4;
              break;
            case "exit":
              num = 7;
              break;
            case "inventory":
              num = 0;
              break;
            case "map":
              num = 3;
              break;
            case "options":
              num = 6;
              break;
            case "powers":
              b.Draw(Game1.mouseCursors_1_6, new Vector2((float) tab.bounds.X, (float) (tab.bounds.Y + (__instance.currentTab == __instance.getTabNumberFromName(tab.name) ? 8 : 0))), new Rectangle?(new Rectangle(216, 494, 16, 16)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
              break;
            case "skills":
              num = 1;
              break;
            case "social":
              num = 2;
              break;
            // (addition)
            case Constants.GameMenuElectricityTabName:
              b.Draw(ModEntry.IconsTexture, new Vector2((float) tab.bounds.X, (float) (tab.bounds.Y + (__instance.currentTab == __instance.getTabNumberFromName(tab.name) ? 8 : 0))), new Rectangle(0, 0, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
              break;
          }
          if (num != -1)
            b.Draw(Game1.mouseCursors, new Vector2((float) tab.bounds.X, (float) (tab.bounds.Y + (__instance.currentTab == __instance.getTabNumberFromName(tab.name) ? 8 : 0))), new Rectangle?(new Rectangle(num * 16, 368, 16, 16)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
          if (tab.name.Equals("skills"))
            Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2((float) (tab.bounds.X + 8), (float) (tab.bounds.Y + 12 + (__instance.currentTab == __instance.getTabNumberFromName(tab.name) ? 8 : 0))), 0.00011f, 3f, 2, Game1.player);
        }
        b.End();
        b.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
        __instance.pages[__instance.currentTab].draw(b);
        if (!__instance.hoverText.Equals(""))
          IClickableMenu.drawHoverText(b, __instance.hoverText, Game1.smallFont);
        
        if (!GameMenu.forcePreventClose && __instance.pages[__instance.currentTab].shouldDrawCloseButton())
            // the following is essentially base.draw(b)
            if (__instance.upperRightCloseButton != null && __instance.shouldDrawCloseButton())
                __instance.upperRightCloseButton.draw(b);
        if (Game1.options.SnappyMenus && (__instance.pages[__instance.currentTab] is CollectionsPage page ? page.letterviewerSubMenu : (LetterViewerMenu) null) != null || Game1.options.hardwareCursor)
            return false;
        __instance.drawMouse(b, true);
        
        return false;
    }
}