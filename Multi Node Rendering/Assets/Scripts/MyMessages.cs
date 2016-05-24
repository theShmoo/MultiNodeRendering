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