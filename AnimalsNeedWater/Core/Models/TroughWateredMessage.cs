namespace AnimalsNeedWater.Core.Models
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