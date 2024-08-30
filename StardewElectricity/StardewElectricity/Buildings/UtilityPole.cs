using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using Constants = StardewElectricity.Utility.Constants;

namespace StardewElectricity.Buildings
{
    public class UtilityPole
    {
        public bool IsPlacedSideways => _building.GetMetadata(Constants.MetadataIsPlacedSideways) == "true";
        public bool IsOrigin => _building.modData.TryGetValue(Constants.ModDataIsOrigin, out var val) && val == "true";
        
        public Vector2 TilePosition
        {
            get => new Vector2(_building.tileX.Value, _building.tileY.Value);
            set { _building.tileX.Set((int)value.X); _building.tileY.Set((int)value.Y); }
        }
        public Vector2 WorldPosition => TilePosition * 16f * 4f;

        public bool HasPower = false;
        
        private Building _building;
        
        public UtilityPole(Building poleBuilding)
        {
            _building = poleBuilding;
        }

        public UtilityPole(Vector2 tile, long owner)
        {
            _building = new Building(Constants.UtilityPoleBuildingTypeName, tile);
            _building.owner.Set(owner);
        }

        public Building GetBuilding()
        {
            return _building;
        }

        public void SetIsPlacedSideways(bool value)
        {
            _building.skinId.Set(value ? Constants.SkinUtilityPoleSide : null);
        }

        public void SetIsOrigin(bool value)
        {
            _building.modData[Constants.ModDataIsOrigin] = value ? "true" : "false";
        }

        /// <summary>
        /// Get the positions (from the top-left corner of the texture) of the electric insulators placed on the pole.
        /// </summary>
        public Tuple<Vector2, Vector2> GetInsulatorPositions()
        {
            if (IsPlacedSideways)
                return new Tuple<Vector2, Vector2>(new Vector2(31, 36), new Vector2(31, 6));

            return new Tuple<Vector2, Vector2>(new Vector2(6, 16), new Vector2(57, 16));
        }


        /// <summary>
        /// Same as GetInsulatorPositions, but with world coordinates.
        /// </summary>
        public Tuple<Vector2, Vector2> GetWorldInsulatorPositions()
        {
            var scale = 4f;

            var insulatorPositions = GetInsulatorPositions();

            var baseCoordinate = new Vector2((int) _building.tileX.Value * 16 * scale + _building.texture.Value.Width / 2f,
                (int) _building.tileY.Value * 16 * scale + (int) _building.tilesHigh.Value * 16 * scale);

            var topLeftCornerTextureCoordinateOnScreen =
                new Vector2(baseCoordinate.X - _building.texture.Value.Width / 2f * scale,
                    baseCoordinate.Y - _building.texture.Value.Height * scale);

            var firstInsulatorPosition =
                new Vector2(topLeftCornerTextureCoordinateOnScreen.X + insulatorPositions.Item1.X * scale,
                    topLeftCornerTextureCoordinateOnScreen.Y + insulatorPositions.Item1.Y * scale);
            var secondInsulatorPosition =
                new Vector2(topLeftCornerTextureCoordinateOnScreen.X + insulatorPositions.Item2.X * scale,
                    topLeftCornerTextureCoordinateOnScreen.Y + insulatorPositions.Item2.Y * scale);

            return new Tuple<Vector2, Vector2>(firstInsulatorPosition, secondInsulatorPosition);

        }
    }
}