using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace StardewElectricity.Utility
{
	public class Utility
	{
		public static void DrawWireWithWidth(SpriteBatch spriteBatch, Vector2 originPoint, Vector2 destinationPoint, int widthInPixels, Func<Vector2, float> layerDepthFunction)
		{
		    var lastPosition = originPoint;
		    
		    var curvaturePoint1 = new Vector2(originPoint.X + (destinationPoint.X - originPoint.X) / 3f, originPoint.Y + (destinationPoint.Y - originPoint.Y) / 3f + 70f);
		    var curvaturePoint2 = new Vector2(originPoint.X + (destinationPoint.X - originPoint.X) * 2f / 3f, originPoint.Y + (destinationPoint.Y - originPoint.Y) * 2f / 3f + 70f);
		    
		    for (float i = 0f; i < 1f; i += 0.025f)
		    {
		        Vector2 currentCurvePoint = StardewValley.Utility.GetCurvePoint(i, originPoint, curvaturePoint1, curvaturePoint2, destinationPoint);

		        for (int j = 0; j <= widthInPixels; j++)
		        {
			        var globalPos1 = new Vector2((int)lastPosition.X + j, (int)lastPosition.Y + j);
			        var globalPos2 = new Vector2((int)currentCurvePoint.X + j, (int)currentCurvePoint.Y + j);
			        var localPos1 = Game1.GlobalToLocal(globalPos1);
			        var localPos2 = Game1.GlobalToLocal(globalPos2);
			        
			        StardewValley.Utility.drawLineWithScreenCoordinates((int)localPos1.X, (int)localPos1.Y, (int)localPos2.X, (int)localPos2.Y, spriteBatch, Color.Black, layerDepthFunction.Invoke(globalPos1));
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