using System;
using System.Text;
using System.Collections.Generic;

using UnityEngine;

public class Packet : IDisposable
{
    private int readPos;
    private List<byte> buffer;
    private byte[] readableBuffer;
    private bool disposed = false;

    public Packet()
    {
        InitializePacket();
    }

    public Packet(ushort id)
    {
        InitializePacket();
        Write(id);
    }

    public Packet(byte[] data)
    {
        InitializePacket();
        SetBytes(data);
    }

    private void InitializePacket()
    {
        buffer = new List<byte>();
        readPos = 0;
    }

    #region Functions
    public void Reset(bool shouldReset = true)
    {
        if (shouldReset)
        {
            buffer.Clear();
            readableBuffer = null;
            readPos = 0;
        }
        else
        {
            readPos -= 4;
        }
    }

    public void SetBytes(byte[] data)
    {
        Write(data);
        readableBuffer = buffer.ToArray();
    }

    public void WriteLength()
    {
        buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
    }

    public void InsertUShort(ushort value)
    {
        buffer.InsertRange(0, BitConverter.GetBytes(value));
    }

    public int Length() => buffer.Count;

    public int UnreadLength() => Length() - readPos;

    public byte[] ToArray() => readableBuffer = buffer.ToArray();
    #endregion

    #region Write Built In Data
    public void Write(sbyte value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(byte value)
    {
        buffer.Add(value);
    }

    public void Write(byte[] value)
    {
        buffer.AddRange(value);
    }

    public void Write(short value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(ushort value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(int value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(uint value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(long value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(ulong value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(float value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(bool value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(string value)
    {
        Write(value.Length);
        buffer.AddRange(Encoding.ASCII.GetBytes(value));
    }
    #endregion

    #region Write Custom Data
    public void Write(Vector2 value)
    {
        Write(value.x);
        Write(value.y);
    }

    public void Write(Vector3 value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
    }

    public void Write(Quaternion value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
        Write(value.w);
    }
    #endregion

    #region Read Built In Data
    public byte ReadByte()
    {
        if (buffer.Count > readPos)
        {
            byte value = readableBuffer[readPos];
            readPos += sizeof(Byte);
            return value;
        }
        else
        {
            throw new Exception($"Can't Read Value Of Type {nameof(Byte)}!");
        }
    }

    public byte[] ReadBytes(int length)
    {
        if (buffer.Count > readPos)
        {
            byte[] value = buffer.GetRange(readPos, length).ToArray();
            readPos += length;
            return value;
        }
        else
        {
            throw new Exception($"Can't Read Value Of Type {nameof(Byte)} Array!");
        }
    }

    public short ReadShort()
    {
        if (buffer.Count > readPos)
        {
            short value = BitConverter.ToInt16(readableBuffer, readPos);
            readPos += sizeof(Int16);
            return value;
        }
        else
        {
            throw new Exception($"Can't Read Value Of Type {nameof(Int16)}!");
        }
    }

    public ushort ReadUShort()
    {
        if (buffer.Count > readPos)
        {
            ushort value = BitConverter.ToUInt16(readableBuffer, readPos);
            readPos += sizeof(UInt16);
            return value;
        }
        else
        {
            throw new Exception($"Can't Read Value Of Type {nameof(UInt16)}!");
        }
    }

    public int ReadInt()
    {
        if (buffer.Count > readPos)
        {
            int value = BitConverter.ToInt32(readableBuffer, readPos);
            readPos += sizeof(Int32);
            return value;
        }
        else
        {
            throw new Exception($"Can't Read Value Of Type {nameof(Int32)}!");
        }
    }

    public uint ReadUInt()
    {
        if (buffer.Count > readPos)
        {
            uint value = BitConverter.ToUInt32(readableBuffer, readPos);
            readPos += sizeof(UInt32);
            return value;
        }
        else
        {
            throw new Exception($"Can't Read Value Of Type {nameof(UInt32)}!");
        }
    }

    public long ReadLong()
    {
        if (buffer.Count > readPos)
        {
            long value = BitConverter.ToInt64(readableBuffer, readPos);
            readPos += sizeof(Int64);
            return value;
        }
        else
        {
            throw new Exception($"Can't Read Value Of Type {nameof(Int64)}!");
        }
    }

    public ulong ReadULong()
    {
        if (buffer.Count > readPos)
        {
            ulong value = BitConverter.ToUInt64(readableBuffer, readPos);
            readPos += sizeof(UInt64);
            return value;
        }
        else
        {
            throw new Exception($"Can't Read Value Of Type {nameof(UInt64)}");
        }
    }

    public float ReadFloat()
    {
        if (buffer.Count > readPos)
        {
            float value = BitConverter.ToSingle(readableBuffer, readPos);
            readPos += sizeof(Single);
            return value;
        }
        else
        {
            throw new Exception($"Can't Read Value Of Type {nameof(Single)}!");
        }
    }

    public bool ReadBoolean()
    {
        if (buffer.Count > readPos)
        {
            bool value = BitConverter.ToBoolean(readableBuffer, readPos);
            readPos += sizeof(Boolean);
            return value;
        }
        else
        {
            throw new Exception($"Can't Read Value Of Type {nameof(Boolean)}!");
        }
    }

    public string ReadString()
    {
        try
        {
            int length = ReadInt();
            string value = Encoding.ASCII.GetString(readableBuffer, readPos, length);
            if (value.Length > 0)
            {
                readPos += length;
            }
            return value;
        }
        catch
        {
            throw new Exception($"Can't Read Value Of Type {nameof(String)}!");
        }
    }
    #endregion

    #region Read Custom Data
    public Vector3 ReadVector2() => new Vector2(ReadFloat(), ReadFloat());
    public Vector3 ReadVector3() => new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
    public Quaternion ReadQuaternion() => new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
    #endregion

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                buffer = null;
                readableBuffer = null;
                readPos = 0;
            }

            disposed = true;
        }
    }
}