using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;

namespace StardewElectricity.Utility
{
    public class Utility
    {
        public static bool CheckIfAbleToBuild(BluePrint structureForPlacement, BuildableGameLocation gameLocation, Vector2 tileLocation, Farmer player)
        {
	        for (int y5 = 0; y5 < structureForPlacement.tilesHeight; y5++)
			{
				for (int x2 = 0; x2 < structureForPlacement.tilesWidth; x2++)
				{
					gameLocation.pokeTileForConstruction(new Vector2(tileLocation.X + (float)x2, tileLocation.Y + (float)y5));
				}
			}
			foreach (Point additionalPlacementTile in structureForPlacement.additionalPlacementTiles)
			{
				int x5 = additionalPlacementTile.X;
				int y4 = additionalPlacementTile.Y;
				gameLocation.pokeTileForConstruction(new Vector2(tileLocation.X + (float)x5, tileLocation.Y + (float)y4));
			}
			for (int y3 = 0; y3 < structureForPlacement.tilesHeight; y3++)
			{
				for (int x3 = 0; x3 < structureForPlacement.tilesWidth; x3++)
				{
					Vector2 currentGlobalTilePosition2 = new Vector2(tileLocation.X + (float)x3, tileLocation.Y + (float)y3);
					if (!gameLocation.isBuildable(currentGlobalTilePosition2))
					{
						return false;
					}
					foreach (Farmer farmer in gameLocation.farmers)
					{
						if (farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x3 * 64, y3 * 64, 64, 64)))
						{
							return false;
						}
					}
				}
			}
			foreach (Point additionalPlacementTile2 in structureForPlacement.additionalPlacementTiles)
			{
				int x4 = additionalPlacementTile2.X;
				int y2 = additionalPlacementTile2.Y;
				Vector2 currentGlobalTilePosition3 = new Vector2(tileLocation.X + (float)x4, tileLocation.Y + (float)y2);
				if (!gameLocation.isBuildable(currentGlobalTilePosition3))
				{
					return false;
				}
				foreach (Farmer farmer2 in gameLocation.farmers)
				{
					if (farmer2.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x4 * 64, y2 * 64, 64, 64)))
					{
						return false;
					}
				}
			}
			if (structureForPlacement.humanDoor != new Point(-1, -1))
			{
				Vector2 doorPos = tileLocation + new Vector2(structureForPlacement.humanDoor.X, structureForPlacement.humanDoor.Y + 1);
				if (!gameLocation.isBuildable(doorPos) && !gameLocation.isPath(doorPos))
				{
					return false;
				}
			}

			return true;
        }

        public static void DrawWireWithWidth(SpriteBatch spriteBatch, Vector2 originPoint, Vector2 destinationPoint, int widthInPixels)
        {
	        var lastPosition = originPoint;
	        
	        //var destinationPoint = Game1.GlobalToLocal(Game1.player.Position);
	        var curvaturePoint1 = new Vector2(originPoint.X + (destinationPoint.X - originPoint.X) / 3f, originPoint.Y + (destinationPoint.Y - originPoint.Y) / 3f + 70f);
	        var curvaturePoint2 = new Vector2(originPoint.X + (destinationPoint.X - originPoint.X) * 2f / 3f, originPoint.Y + (destinationPoint.Y - originPoint.Y) * 2f / 3f + 70f);

	        for (float i = 0f; i < 1f; i += 0.025f)
	        {
		        Vector2 currentCurvePoint = StardewValley.Utility.GetCurvePoint(i, originPoint, curvaturePoint1, curvaturePoint2, destinationPoint);

		        for (int j = 0; j <= widthInPixels; j++)
		        {
			        StardewValley.Utility.drawLineWithScreenCoordinates((int)lastPosition.X + j, (int)lastPosition.Y + j, (int)currentCurvePoint.X + j, (int)currentCurvePoint.Y + j, spriteBatch, Color.Black);
		        }
		        
		        lastPosition = currentCurvePoint;
	        }
        }

        public static int GetTileDistanceBetweenTilePoints(Vector2 tileOne, Vector2 tileTwo)
        {
	        return (int)Vector2.Distance(tileOne, tileTwo);
        }
    }
}