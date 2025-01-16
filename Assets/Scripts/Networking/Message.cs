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

    public MessageType Type;
    private List<byte> writeBuffer;
    private byte[] readBuffer;
    private int readIndex;

    private Message() { }

    // Create a new Message of the specified type. To be used when creating a Message to be sent over a tuple space
    public Message(MessageType type)
    {
        Type = type;
        writeBuffer = new();
    }

    public byte[] ToBytes() => writeBuffer.ToArray();

    public static Message FromBytes(byte[] data)
    {
        Message m = new();
        m.readBuffer = data;
        m.readIndex = 0;
        return m;
    }

    // Create a Message from a tuple. To be used when receiving a tuple from a tuple space to read out values from buffer
    //TODO: Maybe Message.FromTuple() instead?
    public static Message FromTuple(ITuple tuple)
    {
        Message m = new();
        m.Type = MessageTypeHelper.Parse((string)tuple[0]);
        m.readBuffer = (byte[])tuple[1];
        m.readIndex = 0;
        return m;
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

    public void WriteGuid(Guid guid)
    {
        writeBuffer.AddRange(guid.ToByteArray());
    }

    public Guid ReadGuid()
    {
        // Guid is 16 bytes
        if (readIndex + 16 > readBuffer.Length)
        {
            throw new InvalidOperationException("not enough data in Message buffer");
        }
        byte[] guidBytes = new byte[16];
        Array.Copy(readBuffer, readIndex, guidBytes, 0, 16);
        Guid value = new Guid(guidBytes);
        readIndex += 16;
        return value;
    }

    public void WriteEnum(Enum value)
    {
        WriteString(value.ToString());
    }

    public T ReadEnum<T>() where T : struct, Enum
    {
        string value = ReadString();
        if (Enum.TryParse(value, false, out T result))
            return result;
        else
            throw new ArgumentException($"'{value}' is not a valid {typeof(T).Name}");
    }

    public void WriteQuarternion(Quaternion value)
    {
        WriteFloat(value.x);
        WriteFloat(value.y);
        WriteFloat(value.z);
        WriteFloat(value.w);
    }

    public Quaternion ReadQuarternion()
    {
        return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
    }
}