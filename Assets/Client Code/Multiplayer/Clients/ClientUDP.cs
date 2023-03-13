using System;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

public sealed class ClientUDP
{
    public IPEndPoint endPoint;
    public UdpClient Socket { get; set; }

    public EventHandler<UDPClientConnectedArgs> OnClientConnect;

    public void Connect(IPEndPoint _endPoint, int localPort)
    {
        this.endPoint = _endPoint;
        Socket = new UdpClient(localPort);

        Socket.Connect(endPoint);
        Socket.BeginReceive(ReceiveCallback, null);

        var udpArgs = new UDPClientConnectedArgs()
        {
            m_EndPoint = endPoint
        };
        OnClientConnect?.Invoke(this, udpArgs);

        using (Packet packet = new Packet())
        {
            Send(packet);
        }
    }

    public void Send(Packet packet)
    {
        try
        {
            packet.InsertUShort(Client.Instance.id);

            if (Socket != null)
            {
                Socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            byte[] data = Socket.EndReceive(result, ref endPoint);
            Socket.BeginReceive(ReceiveCallback, null);

            if (data.Length < 4)
            {
                Client.Instance.Disconnect();
                return;
            }

            HandleData(data);
        }
        catch
        {
            Disconnect();
        }
    }

    private void HandleData(byte[] data)
    {
        using (Packet packet = new Packet(data))
        {
            int packetLength = packet.ReadInt();
            data = packet.ReadBytes(packetLength);
        }

        ThreadManager.ExecuteOnMainThread(() =>
        {
            using (Packet packet = new Packet(data))
            {
                ushort header = packet.ReadUShort();

                Client.Instance.packetHandlers[header](packet);
            }
        });
    }

    private void Disconnect()
    {
        Client.Instance.Disconnect();

        endPoint = null;
        Socket = null;
    }
}

public class UDPClientConnectedArgs : EventArgs
{
    public IPEndPoint m_EndPoint;
}