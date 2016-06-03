using UnityEngine;
using UnityEngine.Networking;

public class TileIndexMessage : MessageBase
{
    public NetworkInstanceId netId;
    public Vector2 tileIndex;
    public static short MSG_ID = 1000;
}

public class PartTextureMessage : MessageBase
{
    public byte[] data;
    public short id;
    public static short MSG_ID = 1001;
}

public class TileTextureEndMessage : MessageBase
{
    public int tileIndex;
    public int numBytes;
    public static short MSG_ID = 1002;
}

public class TCPConnectionInformation : MessageBase 
{
    /// <summary>
    /// This is the channel id of the tcp channel
    /// </summary>
    public int myReliableChannelId;
    /// <summary>
    /// This is the id of the socket that receives the textures
    /// </summary>
    public int socketId;
    /// <summary>
    /// This is the port of the socket
    /// </summary>
    public int socketPort;
    /// <summary>
    /// The ip adress of the host
    /// </summary>
    public string address;

    public static short MSG_ID = 1003;
}