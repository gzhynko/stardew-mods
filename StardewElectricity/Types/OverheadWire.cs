using Microsoft.Xna.Framework;

namespace StardewElectricity.Types
{
    public class OverheadWire
    {
        public Vector2 Origin { get; set; }
        public Vector2 EndPoint { get; set; }

        public OverheadWire(Vector2 origin, Vector2 endPoint)
        {
            Origin = origin;
            EndPoint = endPoint;
        }
    }
}