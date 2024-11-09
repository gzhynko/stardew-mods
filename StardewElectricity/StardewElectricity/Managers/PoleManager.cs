using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewElectricity.Buildings;
using StardewElectricity.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using Constants = StardewElectricity.Utility.Constants;

namespace StardewElectricity.Managers
{
    public class PoleManager
    {
        public const int WireConnectionTileRange = 15;
        public const int PoleCoverageTileRange = 3;
        
        private List<UtilityPole> _polesOnFarm = new ();
        private List<OverheadWire> _poleWires = new ();

        private Dictionary<int, List<int>> _connectedPoles = new ();
        
        /// <summary>
        /// Tiles that are "powered" (within the pole coverage area)
        /// </summary>
        public HashSet<Vector2> CoveredTiles = new ();
        public HashSet<Guid> CoveredFarmBuildings = new ();
        
        private int _originPoleIndex;
        private Dictionary<Vector2, Guid> _farmBuildingTiles = new ();

        /// <summary>
        /// Called after a save is loaded. Collects all poles placed on the farm into PolesOnFarm.
        /// </summary>
        public void SaveLoaded()
        {
            UpdateFarmBuildingTiles();
            UpdateAll();
        }

        public void UpdateAll()
        {
            UpdatePoles();
            DoWiring();
            CalculatePoleCoverage();
        }
        
        public bool IsTilePowered(GameLocation location, Vector2 tile)
        {
            return (location.ParentBuilding != null && CoveredFarmBuildings.Contains(location.ParentBuilding.id.Value)) 
                   || (location.IsFarm && location.IsOutdoors && CoveredTiles.Contains(tile));
        }

        public void UpdateFarmBuildingTiles()
        {
            _farmBuildingTiles = new Dictionary<Vector2, Guid>();
            
            var farmBuildings = Game1.getFarm().buildings;
            foreach (var building in farmBuildings)
            {
                if (building.buildingType.Value == Constants.UtilityPoleBuildingTypeName)
                    continue;
                
                for (int x = 0; x < building.tilesWide.Value; x++)
                {
                    for (int y = 0; y < building.tilesHigh.Value; y++)
                    {
                        _farmBuildingTiles[new Vector2(building.tileX.Value + x, building.tileY.Value + y)] = building.id.Value;
                    }
                }
            }
        }

        public void UpdatePoles()
        {
            _polesOnFarm = new List<UtilityPole>();
            _originPoleIndex = -1;

            foreach (var building in Game1.getFarm().buildings)
            {
                if (building.buildingType.Value == Constants.UtilityPoleBuildingTypeName)
                {
                    var pole = new UtilityPole(building);
                    _polesOnFarm.Add(pole);
                    
                    if (pole.IsOrigin)
                    {
                        _originPoleIndex = _polesOnFarm.Count - 1;
                    }
                }
            }

            if (_originPoleIndex == -1)
            {
                PlaceOriginPole();
            }
        }

        /// <summary>
        /// Fills out the PoleWires array with positions of the overhead wires.
        /// </summary>
        public void DoWiring(List<UtilityPole>? poles = null)
        {
            poles ??= _polesOnFarm;
            if (poles.Count < 2) return;
            
            _poleWires = new List<OverheadWire>();
            _connectedPoles = new Dictionary<int, List<int>>();
            
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

                        if (_connectedPoles.TryGetValue(i, out var ei))
                            ei.Add(j);
                        else
                            _connectedPoles[i] = new List<int> { j };
                        
                        if (_connectedPoles.TryGetValue(j, out var ej))
                            ej.Add(i);
                        else
                            _connectedPoles[j] = new List<int> { i };
                        
                        _poleWires.Add(wires.Item1);
                        _poleWires.Add(wires.Item2);
                    }

                    pairsSet.Add(new Tuple<int, int>(i, j));
                }
            }
        }

        public void DoWiringConstructionMode(Building poleBuilding)
        {
            var utilityPole = new UtilityPole(poleBuilding);

            var poles = new List<UtilityPole>(_polesOnFarm);
            poles.Add(utilityPole);
            DoWiring(poles);
        }

        /// <summary>
        /// Calculates tiles and farm buildings covered by poles that actually carry power (i.e. connected to the origin pole).
        /// </summary>
        public void CalculatePoleCoverage()
        {
            CoveredTiles = new HashSet<Vector2>();
            CoveredFarmBuildings = new HashSet<Guid>();
            
            // perform a BFS starting from the origin pole
            Queue<int> q = new Queue<int>();
            q.Enqueue(_originPoleIndex);
            HashSet<int> explored = new HashSet<int>();
            while (q.TryPeek(out _))
            {
                var poleIndex = q.Dequeue();
                explored.Add(poleIndex);

                var pole = _polesOnFarm[poleIndex];
                pole.HasPower = true;
                for (int x = -PoleCoverageTileRange; x <= PoleCoverageTileRange; x++)
                {
                    for (int y = -PoleCoverageTileRange; y <= PoleCoverageTileRange; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        var pos = new Vector2(pole.TilePosition.X + x, pole.TilePosition.Y + y);
                        
                        CoveredTiles.Add(pos);
                        if (_farmBuildingTiles.TryGetValue(pos, out var tile))
                            CoveredFarmBuildings.Add(tile);
                    }
                }
                
                if (!_connectedPoles.ContainsKey(poleIndex)) continue;
                foreach (var connectedIndex in _connectedPoles[poleIndex])
                {
                    if (!explored.Contains(connectedIndex))
                    {
                        q.Enqueue(connectedIndex);
                    }
                }
            }
        }
        
        public void DrawWiring(SpriteBatch spriteBatch)
        {
            foreach (var overheadWire in _poleWires)
            {
                var originPoleHeightAboveGround =
                    Math.Abs(overheadWire.OriginPoint.Y - overheadWire.OriginPole.WorldPosition.Y);
                var endPoleHeightAboveGround =
                    Math.Abs(overheadWire.EndPoint.Y - overheadWire.EndPole.WorldPosition.Y);
                
                float LayerDepthFunction(Vector2 pos)
                {
                    bool isOriginPoleClosest = !(Vector2.Distance(pos, overheadWire.EndPole.WorldPosition) <
                                                 Vector2.Distance(pos, overheadWire.OriginPole.WorldPosition));
                    var heightAboveGround =
                        isOriginPoleClosest ? originPoleHeightAboveGround : endPoleHeightAboveGround;
                    return (pos.Y + heightAboveGround - 1f) / 10000f;
                }

                var color = overheadWire.EndPole.HasPower ? Color.DarkBlue : Color.Black;
                
                Utility.Utility.DrawWireWithWidth(spriteBatch, overheadWire.OriginPoint, overheadWire.EndPoint, 2, color, LayerDepthFunction);
            }
        }

        public void DrawPoleCoverage(SpriteBatch spriteBatch)
        {
            if (!Game1.player.currentLocation.IsFarm || !Game1.player.currentLocation.IsOutdoors)
                return;

            foreach (var coveredTileLocation in ModEntry.PoleManager.CoveredTiles)
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, coveredTileLocation * 64f),
                        new Rectangle(194 + 0 * 16, 388, 16, 16),
                    Color.Blue * 0.5f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
            
            foreach (var building in Game1.getFarm().buildings)
            {
                if (!CoveredFarmBuildings.Contains(building.id.Value))
                    continue;
                
                var center = new Vector2(building.tileX.Value * 64f + building.tilesWide.Value * 64f / 2f,
                    building.tileY.Value * 64f + building.tilesHigh.Value * 64f / 2f);
                var scale = 1.5f + (float)Math.Abs(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 800.0f)) / 2f;
                var halfTextureSize = new Vector2(8f, 8f);
                
                spriteBatch.Draw(ModEntry.AssetManager.IconsTexture, Game1.GlobalToLocal(Game1.viewport, center), ModEntry.AssetManager.IconsTextureBoundsMap[IconsTextureItem.BuildingPoweredIcon], Color.White, 0.0f, halfTextureSize, 4f * scale, SpriteEffects.None, 0.9999f);
            }
        }
        
        /// <summary>
        /// If wasn't placed before, place the origin pole.
        /// </summary>
        private void PlaceOriginPole()
        {
            ModEntry.ModMonitor.Log("placeOriginPole", LogLevel.Info);

            var origin = new UtilityPole(new Vector2(83, 6), Game1.player.UniqueMultiplayerID);
            origin.SetIsPlacedSideways(true);
            origin.SetIsOrigin(true);
            
            Game1.getFarm().buildStructure(origin.GetBuilding(), new Vector2(83, 6), Game1.player, true);
        }
        
        private bool AreWithinWireRange(UtilityPole firstPole, UtilityPole secondPole)
        {
            var tileDistance = Utility.Utility.GetTileDistance(firstPole.TilePosition, secondPole.TilePosition);
            return tileDistance <= WireConnectionTileRange;
        }

        private Tuple<OverheadWire, OverheadWire> GetWiresForTwoPoles(UtilityPole firstPole, UtilityPole secondPole)
        {
            var firstPoleInsulatorPositions = firstPole.GetWorldInsulatorPositions();
            var secondPoleInsulatorPositions = secondPole.GetWorldInsulatorPositions();

            Vector2 originPos1 = firstPoleInsulatorPositions.Item1;
            Vector2 endPos1 = secondPoleInsulatorPositions.Item1;
            Vector2 originPos2 = firstPoleInsulatorPositions.Item2;
            Vector2 endPos2 = secondPoleInsulatorPositions.Item2;
            if (firstPole.IsPlacedSideways != secondPole.IsPlacedSideways)
            {
                if ((secondPole.TilePosition.Y > firstPole.TilePosition.Y 
                    && secondPole.TilePosition.X < firstPole.TilePosition.X)
                    || (secondPole.TilePosition.Y < firstPole.TilePosition.Y 
                        && secondPole.TilePosition.X > firstPole.TilePosition.X))
                {
                    endPos1 = secondPoleInsulatorPositions.Item2;
                    endPos2 = secondPoleInsulatorPositions.Item1;
                }
            }

            var firstWire = new OverheadWire(originPos1, firstPole, endPos1, secondPole);
            var secondWire = new OverheadWire(originPos2, firstPole, endPos2, secondPole);

            return new Tuple<OverheadWire, OverheadWire>(firstWire, secondWire);
        }
    }
}