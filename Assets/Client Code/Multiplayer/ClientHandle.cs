using System.Net;
public class ClientHandle {
    public static void OnServerTickReceived(Packet packet)
    {
        Client.Instance.UpdateTick(packet.ReadInt());
    }

    public static void OnPlayerJoinReceived(Packet packet) {
        var id = packet.ReadUShort();
        var isLocalPlayer = packet.ReadBoolean();

        if (isLocalPlayer)
        {
            Client.Instance.id = id;
            Client.Instance.ConnectUdpClient(((IPEndPoint)Client.Instance.tcp.Socket.Client.LocalEndPoint).Port);
        }

        GameManager.Instance.InstantiatePlayer(id, isLocalPlayer, packet.ReadVector3());
    }

    public static void OnPlayerLeaveReceived(Packet packet)
    {
        GameManager.Instance.RemovePlayer(packet.ReadUShort());
    }

    public static void OnPlayerStateReceived(Packet packet)
    {
        var id = packet.ReadUShort();

        var payload = new StatePayload()
        {
            Tick = packet.ReadInt(),
            Time = packet.ReadFloat(),
            Rotation = packet.ReadVector3(),
            Position = packet.ReadVector3()
        };

        if (GameManager.Instance.players[id].isLocalPlayer)
        {
            GameManager.Instance.localPlayer.SetPlayerState(payload);
        }
        else
        {
            GameManager.Instance.remotePlayers[id].AddSnapshot(payload);
        }
    }
}