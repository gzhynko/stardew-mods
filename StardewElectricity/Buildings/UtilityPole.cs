using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace StardewElectricity.Buildings
{
    [XmlType("Mods_StardewElectricity")]
    public class UtilityPole : Building
    {
        public bool IsPlacedSideways = false;
        public bool IsOrigin = false;

        public const string BlueprintName = "UtilityPole";
        private static readonly BluePrint BluePrint = new BluePrint(BlueprintName);

        public Vector2 TilePosition => new Vector2(tileX, tileY);
        private Texture2D TextureToDraw => IsPlacedSideways ? ModEntry.SidewaysPoleTexture : ModEntry.PoleTexture;

        public UtilityPole()
        {
        }
        
        public UtilityPole(bool isOrigin, int tileX, int tileY) : base(BluePrint, new Vector2(tileX, tileY))
        {
            IsOrigin = isOrigin;
        }
        
        public UtilityPole(BluePrint b, Vector2 tileLocation)
            : base(b, tileLocation)
        {
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            if (isMoving)
            {
                return;
            }

            var scaleToDrawAt = 4f;
            
            drawShadow(spriteBatch);
            
            spriteBatch.Draw(TextureToDraw,
                Game1.GlobalToLocal(new Vector2((int) tileX * 16 * scaleToDrawAt + TextureToDraw.Width / 2f,
                    (int) tileY * 16 * scaleToDrawAt + (int) tilesHigh * 16 * scaleToDrawAt)),
                new Rectangle(0, 0, TextureToDraw.Width, TextureToDraw.Height), color.Value * alpha, 0f,
                new Vector2(TextureToDraw.Width / 2f, TextureToDraw.Height), scaleToDrawAt, SpriteEffects.None,
                ((int) tileY + (int) tilesHigh - 1) * 16 * scaleToDrawAt / 10000f);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, int x, int y)
        {
            var scaleToDrawAt = 4f;

            drawShadow(spriteBatch, x, y);
            spriteBatch.Draw(TextureToDraw, new Vector2(x + 16 * scaleToDrawAt / 2, y), getSourceRect(), color, 0f, new Vector2(TextureToDraw.Width / 2f, 0f), 4f, SpriteEffects.None, 0.89f);
        }

        public override void drawShadow(SpriteBatch spriteBatch, int localX = -1, int localY = -1)
        {
            var scaleToDrawAt = 4f;

            var shadowTextureToDraw = ModEntry.PoleShadowTexture;
            
            var shadowOrigin = new Vector2(shadowTextureToDraw.Width / 2f, shadowTextureToDraw.Height / 2f);
            var shadowPosition = localX == -1 ? Game1.GlobalToLocal(new Vector2((int)tileX * 16 * scaleToDrawAt + TextureToDraw.Width / 2f, ((int)tileY + (int)tilesHigh) * 16 * scaleToDrawAt)) : new Vector2(localX + TextureToDraw.Width / 2, localY + getSourceRectForMenu().Height * scaleToDrawAt);

            var rotation = IsPlacedSideways ? (float)Math.PI / 2f : 0f;
            
            spriteBatch.Draw(shadowTextureToDraw, shadowPosition, shadowTextureToDraw.Bounds, Color.White * alpha, rotation, shadowOrigin, scaleToDrawAt, SpriteEffects.None, 1E-05f);
        }

        public static bool Build(BuildableGameLocation gameLocation, Vector2 position, Farmer player)
        {
            if (!Utility.Utility.CheckIfAbleToBuild(BluePrint, gameLocation, position, player))
                return false;

            var building = new UtilityPole(BluePrint, position);
            building.owner.Value = player.UniqueMultiplayerID;
            
            for (int y = 0; y < BluePrint.tilesHeight; y++)
            {
                for (int x = 0; x < BluePrint.tilesWidth; x++)
                {
                    Vector2 currentGlobalTilePosition = new Vector2(position.X + x, position.Y + y);
                    gameLocation.terrainFeatures.Remove(currentGlobalTilePosition);
                }
            }
            
            gameLocation.buildings.Add(building);
            building.performActionOnConstruction(gameLocation);
            
            ModEntry.PoleManager.DoWiring();

            return true;
        }

        /// <summary>
        /// Get the positions (from the top-left corner of the texture) of the electric insulators placed on the pole.
        /// </summary>
        public Tuple<Vector2, Vector2> GetInsulatorPositions()
        {
            if (IsPlacedSideways)
            {
                return new Tuple<Vector2, Vector2>(new Vector2(31, 6), new Vector2(31, 36));
            }
            
            return new Tuple<Vector2, Vector2>(new Vector2(6, 20), new Vector2(57, 20));
        }
        
        
        /// <summary>
        /// Same as GetInsulatorPositions, but with on-screen coordinates.
        /// </summary>
        public Tuple<Vector2, Vector2> GetOnScreenInsulatorPositions()
        {
            var scaleToDrawAt = 4f;

            var insulatorPositions = GetInsulatorPositions();

            var baseCoordinate = new Vector2((int) tileX * 16 * scaleToDrawAt + TextureToDraw.Width / 2f,
                (int) tileY * 16 * scaleToDrawAt + (int) tilesHigh * 16 * scaleToDrawAt);
            
            var topLeftCornerTextureCoordinateOnScreen =
                Game1.GlobalToLocal(new Vector2(baseCoordinate.X - TextureToDraw.Width / 2f * scaleToDrawAt,
                    baseCoordinate.Y - TextureToDraw.Height * scaleToDrawAt));

            var firstInsulatorPosition =
                new Vector2(topLeftCornerTextureCoordinateOnScreen.X + insulatorPositions.Item1.X * scaleToDrawAt,
                    topLeftCornerTextureCoordinateOnScreen.Y + insulatorPositions.Item1.Y * scaleToDrawAt);
            var secondInsulatorPosition =
                new Vector2(topLeftCornerTextureCoordinateOnScreen.X + insulatorPositions.Item2.X * scaleToDrawAt,
                    topLeftCornerTextureCoordinateOnScreen.Y + insulatorPositions.Item2.Y * scaleToDrawAt);

            return new Tuple<Vector2, Vector2>(firstInsulatorPosition, secondInsulatorPosition);

        }
    }
}