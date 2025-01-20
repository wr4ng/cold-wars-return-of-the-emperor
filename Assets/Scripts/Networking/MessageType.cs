using System;

//TODO: Use ints instead of strings as MessageType
//TODO: Maybe split up into Server->Client messages and Client->Server messages to avoid confusion
[Serializable]
public enum MessageType
{
    JoinRequest,
    JoinResponse,
    Disconnect,
    MazeInfo,
    InstatiateNetworkTransform,
    UpdateNetworkTransform,
    DestroyNetworkTransform,
    SetNetworkTransform,
    SpawnBullet,
    PlayerHit,
    NewRound,
}

public static class MessageTypeHelper
{
    public static MessageType Parse(string value)
    {
        if (Enum.TryParse(value, false, out MessageType messageType))
        {
            return messageType;
        }
        else
        {
            throw new ArgumentException($"'{value}' is not a valid MessageType");
        }
    }
}