using System;

namespace AnimalsNeedWater.Types
{
    public class TroughWateredMessage
    {
        public readonly string BuildingUniqueName;

        public TroughWateredMessage(string buildingUniqueName)
        {
            BuildingUniqueName = buildingUniqueName;
        }
    }
}