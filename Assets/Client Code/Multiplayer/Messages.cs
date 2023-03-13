public enum ServerMessage : ushort
{
    serverTick,
    playerJoin,
    playerLeave,
    playerState
}

public enum ClientMessage : ushort
{
    playerInput,
    playerShoot
}