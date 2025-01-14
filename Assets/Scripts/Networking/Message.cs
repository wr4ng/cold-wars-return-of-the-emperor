using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using dotSpace.Interfaces.Space;
using dotSpace.Objects.Space;
using Tuple = dotSpace.Objects.Space.Tuple;

public class Message
{
    public static Pattern MessagePattern = new Pattern(typeof(string), typeof(byte[]));

    public readonly MessageType Type;
    private List<byte> writeBuffer;
    private byte[] readBuffer;
    private int readIndex;

    // Create a new Message of the specified type. To be used when creating a Message to be sent over a tuple space
    public Message(MessageType type)
    {
        Type = type;
        writeBuffer = new();
    }

    // Create a Message from a tuple. To be used when receiving a tuple from a tuple space to read out values from buffer
    public Message(ITuple tuple)
    {
        Type = MessageTypeHelper.Parse((string)tuple[0]);
        readBuffer = (byte[])tuple[1];
        readIndex = 0;
    }

    // Turn Message into a Tuple of the form (MESSAGE_TYPE, DATA) where DATA is a byte[] with values written to Message
    public Tuple ToTuple() => new Tuple(Type.ToString(), writeBuffer.ToArray());

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

    public void WriteBool(bool value)
    {
        writeBuffer.Add((byte)(value ? 1 : 0));
    }

    public bool ReadBool()
    {
        if (readIndex + sizeof(bool) > readBuffer.Length)
        {
            throw new InvalidOperationException("not enough data in Message buffer");
        }
        byte value = readBuffer[readIndex];
        readIndex += 1;
        return (value == (byte)1) ? true : false;
    }

    public void WriteFloat(float value)
    {
        writeBuffer.AddRange(BitConverter.GetBytes(value));
    }

    public float ReadFloat()
    {
        if (readIndex + sizeof(float) > readBuffer.Length)
        {
            throw new InvalidOperationException("not enough data in Message buffer");
        }
        float value = BitConverter.ToSingle(readBuffer, readIndex);
        readIndex += sizeof(float);
        return value;
    }

    public void WriteVector3(Vector3 value)
    {
        WriteFloat(value.x);
        WriteFloat(value.y);
        WriteFloat(value.z);
    }

    public Vector3 ReadVector3()
    {
        return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
    }
}