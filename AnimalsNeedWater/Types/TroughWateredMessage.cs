using System;

namespace AnimalsNeedWater.Types
{
    public class TroughWateredMessage
    {
        public readonly Type BuildingType;
        public readonly string BuildingUniqueName;

        public TroughWateredMessage(Type buildingType, string buildingUniqueName)
        {
            BuildingType = buildingType;
            BuildingUniqueName = buildingUniqueName;
        }
    }
}