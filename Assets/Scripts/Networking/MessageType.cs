using System;

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
    SpawnPowerUp,
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