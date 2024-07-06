using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewElectricity.Buildings;
using StardewElectricity.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using Constants = StardewElectricity.Types.Constants;

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
            ModEntry.ModMonitor.Log("saveLoaded. Init poles and wires", LogLevel.Info);
            UpdatePoles();
            DoWiring(PolesOnFarm);
        }

        public void UpdatePoles()
        {
            PolesOnFarm = new List<UtilityPole>();
            OriginPole = null;

            foreach (var building in Game1.getFarm().buildings)
            {
                if (building.buildingType.Value == Constants.UtilityPoleBuildingTypeName)
                {
                    var pole = new UtilityPole(building);
                    if (pole.IsOrigin)
                    {
                        OriginPole = pole;
                    }
                    
                    PolesOnFarm.Add(pole);
                }
            }

            if (OriginPole == null)
            {
                PlaceOriginPole();
            }
        }

        /// <summary>
        /// Fills out the PoleWires array with positions of the overhead wires.
        /// </summary>
        public void DoWiring(List<UtilityPole> poles)
        {
            if (poles.Count < 2) return;
            
            PoleWires = new List<OverheadWire>();
            ModEntry.ModMonitor.Log($"doing wiring; num of poles on farm: {poles.Count}", LogLevel.Info);

            var pairsSet = new HashSet<Tuple<int, int>>();
            for (int i = 0; i < poles.Count; i++)
            {
                for (int j = i+1; j < poles.Count; j++)
                {
                    if (pairsSet.Contains(new Tuple<int, int>(i,j)) || pairsSet.Contains(new Tuple<int, int>(j,i)))
                        continue;
                    
                    if (AreWithinWireRange(poles[i], poles[j]))
                    {
                        var wires = GetWiresForTwoPoles(poles[i], poles[j]);
                
                        PoleWires.Add(wires.Item1);
                        PoleWires.Add(wires.Item2);
                    }

                    pairsSet.Add(new Tuple<int, int>(i, j));
                }
            }
            
            ModEntry.ModMonitor.Log($"done with wiring; num unique pole pairs: {pairsSet.Count}", LogLevel.Info);
        }

        public void DoWiringConstructionMode(Building poleBuilding)
        {
            var utilityPole = new UtilityPole(poleBuilding);

            var poles = new List<UtilityPole>(PolesOnFarm);
            poles.Add(utilityPole);
            DoWiring(poles);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var overheadWire in PoleWires)
            {
                var originPoleHeightAboveGround =
                    Math.Abs(overheadWire.OriginPoint.Y - overheadWire.OriginPole.WorldPosition.Y);
                var endPoleHeightAboveGround =
                    Math.Abs(overheadWire.EndPoint.Y - overheadWire.EndPole.WorldPosition.Y);
                
                //ModEntry.ModMonitor.Log($"wire depth: {layerDepth}, wire lowest Y: {lowestY}, player depth: {Game1.player.getDrawLayer()}, player Y: {Game1.player.position.Y}", LogLevel.Info);

                float LayerDepthFunction(Vector2 pos)
                {
                    bool isOriginPoleClosest = !(Vector2.Distance(pos, overheadWire.EndPole.WorldPosition) <
                                                 Vector2.Distance(pos, overheadWire.OriginPole.WorldPosition));
                    var heightAboveGround =
                        isOriginPoleClosest ? originPoleHeightAboveGround : endPoleHeightAboveGround;
                    return (pos.Y + heightAboveGround - 1f) / 10000f;
                }

                Utility.Utility.DrawWireWithWidth(spriteBatch, overheadWire.OriginPoint, overheadWire.EndPoint, 2, LayerDepthFunction);
            }
        }
        
        /// <summary>
        /// If wasn't placed before, place the origin pole.
        /// </summary>
        private void PlaceOriginPole()
        {
            OriginPole = new UtilityPole(new Vector2(83, 6), Game1.player.UniqueMultiplayerID);
            OriginPole.SetIsPlacedSideways(true);
            OriginPole.SetIsOrigin(true);
            
            ModEntry.ModMonitor.Log("placeOriginPole", LogLevel.Info);
            
            Game1.getFarm().buildStructure(OriginPole.GetBuilding(), new Vector2(83, 6), Game1.player, true);
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
            return tileDistance <= 15;
        }

        private Tuple<OverheadWire, OverheadWire> GetWiresForTwoPoles(UtilityPole firstPole, UtilityPole secondPole)
        {
            var firstPoleInsulatorPositions = firstPole.GetWorldInsulatorPositions();
            var secondPoleInsulatorPositions = secondPole.GetWorldInsulatorPositions();

            var firstWire = new OverheadWire(firstPoleInsulatorPositions.Item1, firstPole, secondPoleInsulatorPositions.Item1, secondPole);
            var secondWire = new OverheadWire(firstPoleInsulatorPositions.Item2, firstPole, secondPoleInsulatorPositions.Item2, secondPole);

            return new Tuple<OverheadWire, OverheadWire>(firstWire, secondWire);
        }
    }
}