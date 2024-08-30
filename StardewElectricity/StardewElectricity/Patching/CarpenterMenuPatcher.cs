using System;
using Common.Patching;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewElectricity.Managers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using Constants = StardewElectricity.Utility.Constants;

namespace StardewElectricity.Patching;

public class CarpenterMenuPatcher : BasePatcher
{
    private static Vector2 _carpenterOldMouseTilePos = Vector2.Zero; 
    
    public override void Apply(Harmony harmony, IMonitor modMonitor)
    {
        harmony.Patch(
            AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.draw), new []{ typeof(SpriteBatch) }),
            postfix: new HarmonyMethod(this.GetType(), nameof(CarpenterMenuPatcher.Draw_Postfix))
        );
        harmony.Patch(
            AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.receiveKeyPress)),
            prefix: new HarmonyMethod(this.GetType(), nameof(CarpenterMenuPatcher.ReceiveKeyPress_Prefix))
        );
        harmony.Patch(
            AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.receiveLeftClick)),
            prefix: new HarmonyMethod(this.GetType(), nameof(CarpenterMenuPatcher.ReceiveLeftClick_Prefix))
        );
    }
    
    private static void Draw_Postfix(CarpenterMenu __instance, SpriteBatch b)
    {
        if (Game1.IsFading() || __instance.freeze)
        {
            Game1.StartWorldDrawInUI(b);
            ModEntry.PoleManager.DrawWiring(b);
            Game1.EndWorldDrawInUI(b);
            return;
        }
        if (!__instance.onFarm) return;
        if (__instance.upgrading || __instance.demolishing || __instance.painting)
        {
            Game1.StartWorldDrawInUI(b);
            ModEntry.PoleManager.DrawWiring(b);
            Game1.EndWorldDrawInUI(b);
            return;
        }
        
        // ReSharper disable PossibleLossOfFraction
        Vector2 mouseTilePos = new Vector2((Game1.viewport.X + Game1.getOldMouseX(false)) / 64,
            (Game1.viewport.Y + Game1.getOldMouseY(false)) / 64);
        
        // move mode
        if (__instance.moving)
        {
            if (__instance.buildingToMove == null || __instance.buildingToMove.buildingType.Value != Constants.UtilityPoleBuildingTypeName)
                return;
            
            // draw the pole
            __instance.buildingToMove.isMoving = false; // buildings with this flag aren't drawn by the game
            __instance.buildingToMove.tileX.Set((int)mouseTilePos.X);
            __instance.buildingToMove.tileY.Set((int)mouseTilePos.Y);
            
            // update wiring and pole coverage
            if (mouseTilePos != _carpenterOldMouseTilePos)
            {
                ModEntry.PoleManager.UpdateAll();
                _carpenterOldMouseTilePos = mouseTilePos;
            }
            
            // draw pole coverage
            Game1.StartWorldDrawInUI(b);
            ModEntry.PoleManager.DrawPoleCoverage(b);
            Game1.EndWorldDrawInUI(b);
        }
        // build mode
        else
        {
            if (__instance.currentBuilding.buildingType.Value != Constants.UtilityPoleBuildingTypeName)
                return;
            
            int structurePlacementTile = 0;
            
            Game1.StartWorldDrawInUI(b);
            // preview new pole coverage
            for (int x = -PoleManager.PoleCoverageTileRange; x <= PoleManager.PoleCoverageTileRange; x++)
            {
                for (int y = -PoleManager.PoleCoverageTileRange; y <= PoleManager.PoleCoverageTileRange; y++)
                {
                    if (x == 0 && y == 0) continue;
                    Vector2 tileLocation = new Vector2(mouseTilePos.X + x, mouseTilePos.Y + y);
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f),
                        new Microsoft.Xna.Framework.Rectangle?(
                            new Microsoft.Xna.Framework.Rectangle(194 + structurePlacementTile * 16, 388, 16, 16)),
                        Color.Blue * 0.5f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
                }
            }

            // draw existing pole coverage
            ModEntry.PoleManager.DrawPoleCoverage(b);
            
            // draw the pole
            __instance.currentBuilding.tileX.Set((int)mouseTilePos.X);
            __instance.currentBuilding.tileY.Set((int)mouseTilePos.Y);
            __instance.currentBuilding.draw(b);
            Game1.EndWorldDrawInUI(b);
            
            // update wiring
            if (mouseTilePos != _carpenterOldMouseTilePos)
            {
                ModEntry.PoleManager.DoWiringConstructionMode(__instance.currentBuilding);
                _carpenterOldMouseTilePos = mouseTilePos;
            }
            
            Game1.StartWorldDrawInUI(b);
            ModEntry.PoleManager.DrawWiring(b);
            Game1.EndWorldDrawInUI(b);
        }
    }
    
    private static void ReceiveKeyPress_Prefix(CarpenterMenu __instance, Keys key)
    {
        if (Game1.IsFading() || !__instance.onFarm || __instance.freeze) return;
        if (__instance.upgrading || __instance.demolishing || __instance.painting) return;

        if (__instance.moving)
        {
            if (__instance.buildingToMove == null || __instance.buildingToMove.buildingType.Value != Constants.UtilityPoleBuildingTypeName) return;
            
            // rotate (i.e. change skin of) the pole on rotate button press
            if (ModEntry.ModHelper.Input.GetState(SButton.R) == SButtonState.Pressed)
            {
                __instance.buildingToMove.skinId.Set(__instance.buildingToMove.skinId.Value == null ? Constants.SkinUtilityPoleSide : null);
                __instance.buildingToMove.resetTexture();
                _carpenterOldMouseTilePos = Vector2.Zero; // reset old mouse pos to update wiring
            }
        }
        else
        {
            if (__instance.currentBuilding.buildingType.Value != Constants.UtilityPoleBuildingTypeName)
                return;
            // update wiring when closing without successful build
            if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && __instance.readyToClose() && Game1.locationRequest == null)
            {
                __instance.returnToCarpentryMenu();
                ModEntry.PoleManager.DoWiring();
            }
            // TODO: Make button configurable
            // rotate (i.e. change skin of) the pole on rotate button press
            if (ModEntry.ModHelper.Input.GetState(SButton.R) == SButtonState.Pressed)
            {
                __instance.currentBuilding.skinId.Set(__instance.currentBuilding.skinId.Value == null ? Constants.SkinUtilityPoleSide : null);
                __instance.currentBuilding.resetTexture();
                _carpenterOldMouseTilePos = Vector2.Zero; // reset old mouse pos to update wiring
            }
        }
    }

    private static bool ReceiveLeftClick_Prefix(CarpenterMenu __instance, int x, int y, bool playSound = true)
    {
        if (__instance.freeze || !__instance.onFarm)
            return true;
        if (__instance.cancelButton.containsPoint(x, y))
            return true;
        if (__instance.upgrading || __instance.painting)
            return true;

        if (__instance.moving)
        {
            // only affect poles
            if (__instance.buildingToMove == null || __instance.buildingToMove.buildingType.Value != Utility.Constants.UtilityPoleBuildingTypeName)
                return true;
            
            Vector2 vector2 = new Vector2((float) ((Game1.viewport.X + Game1.getMouseX(false)) / 64), (float) ((Game1.viewport.Y + Game1.getMouseY(false)) / 64));
            if (__instance.ConfirmBuildingAccessibility(vector2))
            {
                __instance.buildingToMove.isMoving = true; // temporarily enable moving flag to do safety checks
                __instance.buildingToMove.tileX.Set(0);
                __instance.buildingToMove.tileY.Set(0);
                
                if (__instance.TargetLocation.buildStructure(__instance.buildingToMove, vector2, Game1.player))
                {
                    __instance.buildingToMove.isMoving = false;
                    __instance.buildingToMove = (Building) null;
                    Game1.playSound("axchop");
                    DelayedAction.playSoundAfterDelay("dirtyHit", 50);
                    DelayedAction.playSoundAfterDelay("dirtyHit", 150);
                    
                    ModEntry.PoleManager.UpdateAll();
                }
                else
                    Game1.playSound("cancel");
            }
            else
            {
                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), 3));
                Game1.playSound("cancel");
            }
        }
        else if (__instance.demolishing)
        {
            GameLocation farm = __instance.TargetLocation;
            Building destroyed = farm.getBuildingAt(new Vector2((float) ((Game1.viewport.X + Game1.getOldMouseX(false)) / 64), (float) ((Game1.viewport.Y + Game1.getOldMouseY(false)) / 64)));
            if (destroyed == null)
                return false;
            // only affect poles
            if (destroyed.buildingType.Value != Constants.UtilityPoleBuildingTypeName)
                return true;
            
            if (!__instance.CanDemolishThis(destroyed))
                return false;
            if (!Game1.IsMasterGame && !__instance.hasPermissionsToDemolish(destroyed))
                return false;
            Game1.player.team.demolishLock.RequestLock(() =>
            {
                if (!__instance.demolishing || destroyed == null || !farm.buildings.Contains(destroyed))
                    return;
                destroyed.BeforeDemolish();
                if (!farm.destroyStructure(destroyed))
                    return;
                destroyed.showDestroyedAnimation(__instance.TargetLocation);
                Game1.playSound("explosion");
                
                if (!ModEntry.ModHelper.Input.IsDown(SButton.LeftShift))
                {
                    DelayedAction.functionAfterDelay(new Action(__instance.returnToCarpentryMenu), 1500);
                    __instance.freeze = true;
                }
            }, () => {});
        }
        // build mode
        else
        {
            // only affect poles
            if (__instance.currentBuilding.buildingType.Value != Constants.UtilityPoleBuildingTypeName)
                return true;
            Game1.player.team.buildLock.RequestLock((Action) (() =>
            {
                if (__instance.onFarm && Game1.locationRequest == null)
                {
                    if (!__instance.DoesFarmerHaveEnoughResourcesToBuild())
                    {
                        Game1.addHUDMessage(new HUDMessage("Not Enough Resources", 3));
                        return;
                    }

                    if (__instance.tryToBuild())
                    {
                        __instance.ConsumeResources();

                        if (!ModEntry.ModHelper.Input.IsDown(SButton.LeftShift))
                        {
                            DelayedAction.functionAfterDelay(new Action(__instance.returnToCarpentryMenuAfterSuccessfulBuild), 2000);
                            __instance.freeze = true;
                        }
                    }
                    else
                        Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), 3));
                }
                Game1.player.team.buildLock.ReleaseLock();
            }));
        }
        
        return false;
    }
}