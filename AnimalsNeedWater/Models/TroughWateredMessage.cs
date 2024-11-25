using System;

namespace AnimalsNeedWater.Models
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