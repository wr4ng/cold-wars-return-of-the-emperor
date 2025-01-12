using System;
using System.Collections.Generic;
using System.Data.Common;

public class Message
{
    private List<byte> writeBuffer;
    private byte[] readBuffer;
    private int readIndex;

    public Message()
    {
        writeBuffer = new();
    }

    public Message(byte[] data)
    {
        readBuffer = data;
        readIndex = 0;
    }

    public byte[] ToArray() => writeBuffer.ToArray();

    public void WriteInt(int value)
    {
        writeBuffer.AddRange(BitConverter.GetBytes(value));
    }

    public int ReadInt()
    {
        if (readIndex + sizeof(int) > readBuffer.Length)
        {
            throw new InvalidOperationException("not enough data in Message buffer");
        }
        int value = BitConverter.ToInt32(readBuffer, readIndex);
        readIndex += sizeof(int);
        return value;
    }

}