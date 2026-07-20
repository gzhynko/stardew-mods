using System.Collections.Generic;

namespace AnimalsNeedWater.Core.Models;

public class RequestStateMessage
{
    public readonly long RequesterId;

    public RequestStateMessage(long requesterId)
    {
        RequesterId = requesterId;
    }

}