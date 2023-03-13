using System;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

public sealed class ClientTCP
{
    #region Events
    public event EventHandler OnClientDisconnect;
    public event EventHandler<TCPClientConnectedArgs> OnClientConnect;
    #endregion

    #region Fields
    public IPEndPoint endPoint;

    private byte[] receiveBuffer;
    private Packet receivedPacket;
    #endregion

    #region Propertys
    public TcpClient Socket { get; set; }
    public NetworkStream Stream { 
        get => Socket.GetStream();
        private set => Stream = value;
    }
    #endregion

    public void Connect(IPEndPoint endPoint)
    {
        this.endPoint = endPoint;

        InitializeClientData();

        try
        {
            Socket.Connect(this.endPoint);

            var args = new TCPClientConnectedArgs()
            {
                m_EndPoint = this.endPoint
            };

            OnClientConnect?.Invoke(this, args);
            OnClientDisconnect?.Invoke(this, args);

            Stream.BeginRead(receiveBuffer, 0, Constants.DATA_BUFFER_SIZE, DataReceiveCallback, null);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void InitializeClientData()
    {
        Socket = new TcpClient()
        {
            SendBufferSize = Constants.DATA_BUFFER_SIZE,
            ReceiveBufferSize = Constants.DATA_BUFFER_SIZE
        };

        receivedPacket = new Packet();
        receiveBuffer = new byte[Constants.DATA_BUFFER_SIZE];
    }

    private void DataReceiveCallback(IAsyncResult result)
    {
        try
        {
            int dataReadLength = Stream.EndRead(result);

            if (dataReadLength <= 0)
            {
                Disconnect();
                return;
            }

            byte[] buffer = new byte[dataReadLength];
            Array.Copy(receiveBuffer, buffer, dataReadLength);

            receivedPacket.Reset(HandleData(buffer));
            Stream.BeginRead(receiveBuffer, 0, Constants.DATA_BUFFER_SIZE, DataReceiveCallback, null);
        }
        catch
        {
            Disconnect();
        }
    }

    private bool HandleData(byte[] buffer)
    {
        int packetLength = 0;

        receivedPacket.SetBytes(buffer);

        if (receivedPacket.UnreadLength() >= 4)
        {
            packetLength = receivedPacket.ReadInt();
            if (packetLength <= 0)
            {
                return true;
            }
        }

        while (packetLength > 0 && packetLength <= receivedPacket.UnreadLength())
        {
            byte[] packetBytes = receivedPacket.ReadBytes(packetLength);
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new(packetBytes))
                {
                    ushort header = packet.ReadUShort();

                    Client.Instance.packetHandlers[header](packet);
                }
            });

            packetLength = 0;

            if (receivedPacket.UnreadLength() >= 4)
            {
                packetLength = receivedPacket.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }
        }

        if (packetLength <= 1)
        {
            return true;
        }

        return false;
    }

    public void Send(Packet packet)
    {
        try
        {
            if (Socket != null)
            {
                Stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            }
        }
        catch
        {
        }
    }

    public void Disconnect()
    {
        Client.Instance.Disconnect();

        Stream = null;
        receiveBuffer = null;
        receivedPacket = null;
        Socket = null;
    }
}

public class TCPClientConnectedArgs : EventArgs
{
    public IPEndPoint m_EndPoint { get; set; }
}