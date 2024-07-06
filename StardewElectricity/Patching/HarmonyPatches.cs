using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using Constants = StardewElectricity.Types.Constants;

// ReSharper disable InconsistentNaming

namespace StardewElectricity.Patching
{
    public class HarmonyPatches
    {
        private static Vector2 carpenterOldMouseTilePos = Vector2.Zero; 
        
        /// <summary> Patch for the CarpenterMenu.setNewActiveBlueprint method. </summary>
        public static bool BuildingDrawShadow(Building __instance, SpriteBatch b, int localX = -1, int localY = -1)
        {
            // apply shadow changes to the utility pole building
            if (__instance.buildingType.Value != Constants.UtilityPoleBuildingTypeName)
                return true; // run original method
            
            Rectangle rectangle = __instance.getSourceRectForMenu() ?? __instance.getSourceRect();
            Vector2 offset = new Vector2(8.0f * 4.0f, -8.0f * 4.0f);
            Vector2 position = 
                localX == -1 ? Game1.GlobalToLocal(new Vector2(__instance.tileX.Value * 16 * 4, (__instance.tileY.Value + __instance.tilesHigh.Value) * 16 * 4) + offset) 
                    : new Vector2(localX + 32.0f * 4.0f, localY + rectangle.Height * 4);
            
            var rotationCenter = new Vector2(ModEntry.PoleShadowTexture.Width / 2.0f,
                ModEntry.PoleShadowTexture.Height / 2.0f);
            // apply a 90deg rotation if placed sideways
            var shadowRotation = __instance.GetMetadata(Constants.MetadataIsPlacedSideways) == "true" ? (float)Math.PI / 2.0f : 0.0f;

            // alpha is a protected field, so we use a reflection
            var alpha = localX == -1
                ? ModEntry.Instance.Helper.Reflection.GetField<float>(__instance, "alpha").GetValue()
                : 1f;
            
            b.Draw(ModEntry.PoleShadowTexture, position, null, Color.White * alpha, shadowRotation, rotationCenter, 4f, SpriteEffects.None, 1E-05f);

            return false; // don't run original method
        }

        public static void CarpenterMenuDraw(CarpenterMenu __instance, SpriteBatch b)
        {
            if (Game1.IsFading() || __instance.freeze)
            {
                ModEntry.PoleManager.Draw(b);
                return;
            }
            if (!__instance.onFarm || __instance.currentBuilding.buildingType.Value != Constants.UtilityPoleBuildingTypeName) return;
            
            // build mode
            if (!__instance.upgrading && !__instance.demolishing && !__instance.moving && !__instance.painting)
            {
                // ReSharper disable PossibleLossOfFraction
                Vector2 mouseTilePos = new Vector2((Game1.viewport.X + Game1.getOldMouseX(false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(false)) / 64);
                int structurePlacementTile = 0;
                
                Game1.StartWorldDrawInUI(b);
                for (int x = -3; x <= 3; x++)
                {
                    for (int y = -3; y <= 3; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        
                        Vector2 tileLocation = new Vector2(mouseTilePos.X + x, mouseTilePos.Y + y);
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(194 + structurePlacementTile * 16, 388, 16, 16)), Color.Blue * 0.5f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
                    }
                }
                __instance.currentBuilding.tileX.Set((int)mouseTilePos.X);
                __instance.currentBuilding.tileY.Set((int)mouseTilePos.Y);
                __instance.currentBuilding.draw(b);

                if (mouseTilePos != carpenterOldMouseTilePos)
                {
                    ModEntry.PoleManager.DoWiringConstructionMode(__instance.currentBuilding);
                    carpenterOldMouseTilePos = mouseTilePos;
                }

                ModEntry.PoleManager.Draw(b);
                
                Game1.EndWorldDrawInUI(b);
            }
        }
    }
}