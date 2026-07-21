namespace AnimalsNeedWater.Core.Multiplayer.Models;

public class BonusAwardedMessage
{
    public readonly string BuildingUniqueName;

    public BonusAwardedMessage(string buildingUniqueName)
    {
        BuildingUniqueName = buildingUniqueName;
    }
}