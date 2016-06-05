using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// This class represents a state structure for the scene/animation for distributed rendering systems to transmit all neccessary parameters to the render node
/// </summary>
public class RayCastStateMessage : MessageBase
{
    public long deltaTime;

    public Matrix4x4 viewMatrix;

    public Matrix4x4 projectionMatrix;

    public Matrix4x4 volumeWorldMatrix;

    public static short MSG_ID = 1000;
}

public class TileMessage : MessageBase
{
    public float fov;

    public float np;

    public float fp;

    public float aspect;

    public int screenWidth;

    public int screenHeight;

    public Vector2 numTiles;

    public Vector2 tileIndex;

    public static short MSG_ID = 1001;

    // This method would be generated
    public override void Deserialize(NetworkReader reader)
    {
        fov = reader.ReadSingle();
        np = reader.ReadSingle();
        fp = reader.ReadSingle();
        aspect = reader.ReadSingle();

        screenWidth = reader.ReadInt32();
        screenHeight = reader.ReadInt32();

        numTiles = reader.ReadVector2();
        tileIndex = reader.ReadVector2();
    }

    // This method would be generated
    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(fov);
        writer.Write(np);
        writer.Write(fp);
        writer.Write(aspect);

        writer.Write(screenWidth);
        writer.Write(screenHeight);

        writer.Write(numTiles);
        writer.Write(tileIndex);
    }
}

public class RenderParameterMessage : MessageBase
{
    public int pass;

    public float opacity;

    public static short MSG_ID = 1002;
}
