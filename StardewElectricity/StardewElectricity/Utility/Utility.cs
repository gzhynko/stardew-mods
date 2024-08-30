using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewElectricity.Types;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace StardewElectricity.Utility
{
	public class Utility
	{
		public static void DrawWireWithWidth(SpriteBatch spriteBatch, Vector2 originPoint, Vector2 destinationPoint, int widthInPixels, Color color, Func<Vector2, float> layerDepthFunction)
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
			        if (!Game1.viewport.Contains(new Location((int)globalPos1.X, (int)globalPos1.Y))
			            && !Game1.viewport.Contains(new Location((int)globalPos2.X, (int)globalPos2.Y)))
						continue;
			        
			        var localPos1 = Game1.GlobalToLocal(globalPos1);
			        var localPos2 = Game1.GlobalToLocal(globalPos2);
			        
			        StardewValley.Utility.drawLineWithScreenCoordinates((int)localPos1.X, (int)localPos1.Y, (int)localPos2.X, (int)localPos2.Y, spriteBatch, color, layerDepthFunction.Invoke(globalPos1));
		        }
		        
		        lastPosition = currentCurvePoint;
		    }
		}

		public static int GetTileDistance(Vector2 tileOne, Vector2 tileTwo)
		{
			return (int)Vector2.Distance(tileOne, tileTwo);
		}

		public static int GetBillingCycleLengthDays(BillingCycle cycle)
		{
			switch (cycle)
			{
				case BillingCycle.Daily:
					return 1;
				case BillingCycle.Monthly:
					return 28;
				case BillingCycle.Weekly:
					return 7;
			}

			return -1;
		}
		
		public static string GetBillingCycleContextString(BillingCycle cycle)
		{
			switch (cycle)
			{
				case BillingCycle.Daily:
					return "today";
				case BillingCycle.Monthly:
					return "this month";
				case BillingCycle.Weekly:
					return "this week";
			}

			return "";
		}

		public static WorldDate TotalDaysToWorldDate(int totalDays)
		{
			var year = totalDays % WorldDate.DaysPerYear;
			var season = totalDays % WorldDate.DaysPerMonth % WorldDate.MonthsPerYear;
			var dayOfMonth = totalDays % WorldDate.DaysPerMonth;
			return new WorldDate(year, (Season)season, dayOfMonth);
		}

		public static Dictionary<string, string> GetCustomFields(string qualifiedItemId)
		{
			var itemData = ItemRegistry.GetDataOrErrorItem(qualifiedItemId);
			dynamic rawData = itemData.RawData;
			
			return rawData == null ? null : (Dictionary<string, string>)rawData.CustomFields;
		}
		
		public static bool IsConsumer(string qualifiedItemId)
		{
			var customFields = GetCustomFields(qualifiedItemId);
			return customFields != null && customFields.ContainsKey(Constants.ModDataKwhConsumedPer10Minutes);
		}

		public static float? GetKwhConsumedPer10Minutes(string qualifiedItemId)
		{
			var customFields = GetCustomFields(qualifiedItemId);
			var value = customFields?[Constants.ModDataKwhConsumedPer10Minutes];
			if (value == null)
				return null;

			if (float.TryParse(value, out var parsed))
				return parsed;
			return null;
		}
	}
}