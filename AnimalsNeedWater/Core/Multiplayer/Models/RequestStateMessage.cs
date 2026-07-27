namespace AnimalsNeedWater.Core.Multiplayer.Models;

public class RequestStateMessage
{
    public readonly long RequesterId;

    public RequestStateMessage(long requesterId)
    {
        RequesterId = requesterId;
    }

}