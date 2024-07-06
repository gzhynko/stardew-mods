using Microsoft.Xna.Framework;
using StardewElectricity.Buildings;

namespace StardewElectricity.Types
{
    public class OverheadWire
    {
        public Vector2 OriginPoint { get; set; }
        public UtilityPole OriginPole { get; set; }
        public Vector2 EndPoint { get; set; }
        public UtilityPole EndPole { get; set; }

        public OverheadWire(Vector2 origin, UtilityPole opole, Vector2 endPoint, UtilityPole epole)
        {
            OriginPoint = origin;
            OriginPole = opole;
            EndPoint = endPoint;
            EndPole = epole;
        }
    }
}