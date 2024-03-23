using System;

namespace AnimalsNeedWater.Types
{
    public class TroughWateredMessage
    {
        public readonly string BuildingType;
        public readonly string BuildingUniqueName;

        public TroughWateredMessage(string buildingType, string buildingUniqueName)
        {
            BuildingType = buildingType;
            BuildingUniqueName = buildingUniqueName;
        }
    }
}