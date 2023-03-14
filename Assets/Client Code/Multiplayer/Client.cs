using System;
using System.Net;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public struct ServerData {
    public int port;
    public string ipAddress;
}

public class Client : MonoBehaviour {
    public ServerData serverData;

    public ClientTCP tcp;
    public ClientUDP udp;
    
    public IPEndPoint endPoint;

    public ushort id;
    public bool isConnected;

    public delegate void PacketHandler(Packet packet);    
    public Dictionary<ushort, PacketHandler> packetHandlers;

    public int serverTick;

    // Singleton

    private static Client instance;
    public static Client Instance
    {
        get => instance;
        set
        {
            if (instance == null)
            {
                instance = value;
            }
            else if (instance != value)
            {
                Destroy(value);
            }
        }
    }

    void Awake() {
        Instance = this;
    }

    void Start() {
        IPAddress address = IPAddress.Parse(serverData.ipAddress);
        endPoint = new(address, serverData.port);

        ConnectTcpClient();
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }

    public void InitializePackets()
    {
        packetHandlers = new Dictionary<ushort, PacketHandler>() {
            { (ushort)ServerMessage.serverTick, ClientHandle.OnServerTickReceived },
            { (ushort)ServerMessage.playerJoin, ClientHandle.OnPlayerJoinReceived },
            { (ushort)ServerMessage.playerLeave, ClientHandle.OnPlayerLeaveReceived },
            { (ushort)ServerMessage.playerState, ClientHandle.OnPlayerStateReceived },
        };
    }

    public void ConnectTcpClient()
    {
        InitializePackets();

        tcp = new();
        tcp.OnClientConnect += (object sender, TCPClientConnectedArgs args) => {
            isConnected = true;
            Debug.Log($"TCP connected {args.m_EndPoint}");
        };
        tcp.Connect(endPoint);
    }

    public void ConnectUdpClient(int localPort)
    {
        udp = new();
        udp.OnClientConnect += (object sender, UDPClientConnectedArgs args) => {
            Debug.Log($"UDP connected {args.m_EndPoint}");
        };
        udp.Connect(endPoint, localPort);
    }

    public void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;

            tcp.Socket.Close();
            udp.Socket.Close();

            Debug.Log("Disconnected");
        }
    }

    public void UpdateTick(int tick) => serverTick = tick;
}