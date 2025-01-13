using System;

//TODO: Use ints instead of strings as MessageType
public enum MessageType
{
    Hello, //TODO: Remove once unused
    JoinRequest,
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