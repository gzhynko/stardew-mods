using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewElectricity.Buildings;
using StardewElectricity.Types;
using StardewModdingAPI;
using StardewValley;

namespace StardewElectricity.Managers
{
    public class PoleManager
    {
        public List<UtilityPole> PolesOnFarm = new List<UtilityPole>();

        public List<OverheadWire> PoleWires = new List<OverheadWire>();

        public UtilityPole OriginPole;

        /// <summary>
        /// Called after a save is loaded. Collects all of the poles placed on the farm into PolesOnFarm.
        /// </summary>
        public void SaveLoaded()
        {
            ModEntry.ModMonitor.Log("saveLoaded", LogLevel.Info);
            foreach (var building in Game1.getFarm().buildings)
            {
                ModEntry.ModMonitor.Log(building.buildingType, LogLevel.Info);

                if (building is UtilityPole pole)
                {
                    if (pole.IsOrigin)
                    {
                        OriginPole = pole;
                        return;
                    }
                    
                    PolesOnFarm.Add(pole);
                    
                    ModEntry.ModMonitor.Log("pole", LogLevel.Info);
                }
            }

            if (OriginPole == null)
            {
                PlaceOriginPole();
            }
            
            DoWiring();
        }

        /// <summary>
        /// Fills out the PoleWires array with positions of the overhead wires.
        /// </summary>
        public void DoWiring()
        {
            if (PolesOnFarm.Count < 1) return;

            ModEntry.ModMonitor.Log("wiring", LogLevel.Info);

            var closestPoleToOrigin = GetClosestUtilityPole(OriginPole);
            if (closestPoleToOrigin == null) return;
            
            ModEntry.ModMonitor.Log("wiring2", LogLevel.Info);

            if (AreWithinWireRange(OriginPole, closestPoleToOrigin))
            {
                var wires = GetWiresForTwoPoles(OriginPole, closestPoleToOrigin);
                
                PoleWires.Add(wires.Item1);
                PoleWires.Add(wires.Item2);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(PoleWires.Count < 1) return;

            foreach (var overheadWire in PoleWires)
            {
                Utility.Utility.DrawWireWithWidth(spriteBatch, overheadWire.Origin, overheadWire.EndPoint, 2);
            }
        }
        
        /// <summary>
        /// If wasn't placed before, place the origin pole.
        /// </summary>
        private void PlaceOriginPole()
        {
            OriginPole = new UtilityPole(true, 76, 12);
            
            OriginPole.IsPlacedSideways = true;
            OriginPole.magical.Value = true;
            OriginPole.owner.Value = Game1.player.UniqueMultiplayerID;
            
            ModEntry.ModMonitor.Log("placeOriginPole", LogLevel.Info);
            
            Game1.getFarm().buildings.Add(OriginPole); 
            Game1.getFarm().buildings.Last().daysOfConstructionLeft.Value = 0;
        }

        private UtilityPole GetClosestUtilityPole(UtilityPole previousPole)
        {
            var closestNumberYet = int.MaxValue;
            UtilityPole closestPoleYet = null;
            
            foreach (var pole in PolesOnFarm)
            {
                if(pole == previousPole) continue;
                
                if (Utility.Utility.GetTileDistanceBetweenTilePoints(previousPole.TilePosition, pole.TilePosition) <
                    closestNumberYet)
                {
                    closestPoleYet = pole;
                }
            }

            return closestPoleYet;
        }

        private bool AreWithinWireRange(UtilityPole firstPole, UtilityPole secondPole)
        {
            var tileDistance = Utility.Utility.GetTileDistanceBetweenTilePoints(firstPole.TilePosition, secondPole.TilePosition);

            ModEntry.ModMonitor.Log(tileDistance.ToString(), LogLevel.Debug);
            
            return tileDistance <= 10;
        }

        private Tuple<OverheadWire, OverheadWire> GetWiresForTwoPoles(UtilityPole firstPole, UtilityPole secondPole)
        {
            var firstPoleInsulatorPositions = firstPole.GetOnScreenInsulatorPositions();
            var secondPoleInsulatorPositions = secondPole.GetOnScreenInsulatorPositions();

            var firstWire = new OverheadWire(firstPoleInsulatorPositions.Item1, secondPoleInsulatorPositions.Item1);
            var secondWire = new OverheadWire(firstPoleInsulatorPositions.Item2, secondPoleInsulatorPositions.Item2);

            return new Tuple<OverheadWire, OverheadWire>(firstWire, secondWire);
        }
    }
}