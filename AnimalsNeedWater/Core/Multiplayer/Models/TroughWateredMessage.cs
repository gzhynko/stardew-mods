namespace AnimalsNeedWater.Core.Multiplayer.Models;

public class TroughWateredMessage
{
    public readonly string BuildingUniqueName;

    public TroughWateredMessage(string buildingUniqueName)
    {
        BuildingUniqueName = buildingUniqueName;
    }
}