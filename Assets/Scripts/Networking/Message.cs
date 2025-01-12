using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

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

    public void WriteString(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        WriteInt(bytes.Length);
        writeBuffer.AddRange(bytes);
    }

    public string ReadString()
    {
        if (readIndex + sizeof(int) > readBuffer.Length)
        {
            throw new InvalidOperationException("not enough data in Message buffer");
        }
        int length = ReadInt();
        if (readIndex + length > readBuffer.Length)
        {
            throw new InvalidOperationException("not enough data in Message buffer");
        }
        string value = Encoding.UTF8.GetString(readBuffer, readIndex, length);
        readIndex += length;
        return value;
    }

}