public static class ClientSend
{
    public static void SendTcpData(Packet packet)
    {
        packet.WriteLength();
        Client.Instance.tcp.Send(packet);
    }

    public static void SendUdpData(Packet packet)
    {
        packet.WriteLength();
        Client.Instance.udp.Send(packet);
    }

    public static void PlayerInput(InputPayload payload)
    {
        using (Packet packet = new Packet((ushort)ClientMessage.playerInput))
        {
            packet.Write(Client.Instance.id);
            packet.Write(payload.Tick);
            packet.Write(payload.Time);
            packet.Write(payload.Input);
            packet.Write(payload.Rotation);

            SendTcpData(packet);
        }
    }

    public static void PlayerShoot(ShootSnapshot snapshot)
    {
        using (Packet packet = new Packet((ushort)ClientMessage.playerShoot))
        {
            packet.Write(Client.Instance.id);
            packet.Write(snapshot.Tick);

            SendTcpData(packet);
        }
    }
}