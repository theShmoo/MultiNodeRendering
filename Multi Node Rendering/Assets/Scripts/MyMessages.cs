using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// This class represents a state structure for the scene/animation for distributed rendering systems to transmit all neccessary parameters to the render node
/// </summary>
public class SceneStateMessage : MessageBase
{
    /// <summary>
    /// The current camera position
    /// </summary>
    public Vector3 cameraPos;

    /// <summary>
    /// The current view matrix
    /// </summary>
    public Matrix4x4 viewMatrix;

    /// <summary>
    /// The current view projection matrix
    /// </summary>
    public Matrix4x4 projectionMatrix;

    /// <summary>
    /// The current volume world matrix
    /// </summary>
    public Matrix4x4 volumeWorldMatrix;

    /// <summary>
    /// The Network message id to identify the message
    /// </summary>    
    public static short MSG_ID = 1000;
}

/// <summary>
/// This class represents a network message that specifies one tile. 
/// </summary>
/// <see cref="ScreenTile" />
public class TileMessage : MessageBase
{
    /// <summary>
    /// Field of View
    /// </summary>
    public float fov;

    /// <summary>
    /// Near Plane
    /// </summary>    
    public float np;

    /// <summary>
    /// Far Plane
    /// </summary>
    public float fp;

    /// <summary>
    /// Aspect Ratio 
    /// </summary>
    public float aspect;

    /// <summary>
    /// The screen width of the composer
    /// </summary>
    public int screenWidth;

    /// <summary>
    /// The screen height of the composer
    /// </summary>
    public int screenHeight;

    /// <summary>
    /// The number of tiles in x and y direction
    /// </summary>
    public Vector2 numTiles;

    /// <summary>
    /// The index of this tile in the x and y direction
    /// </summary>
    public Vector2 tileIndex;

    /// <summary>
    /// The Network message id to identify the message
    /// </summary>
    public static short MSG_ID = 1001;

    /// <summary>
    /// Deserialize the message parameters
    /// This method would be generated
    /// </summary>
    /// <param name="reader">the network stream writer</param>
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

    /// <summary>
    /// Serialize the message parameters
    /// This method would be generated
    /// </summary>
    /// <param name="writer">the network stream writer</param>
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

/// <summary>
/// This class represents a network message to set the raycast render parameters.
/// </summary>
public class RaycastParameterMessage : MessageBase
{
    /// <summary>
    /// Which pass the raycast shader should use
    /// </summary>
    public int pass;

    /// <summary>
    /// The opacity of the raycast shader
    /// </summary>
    public float opacity;

    /// <summary>
    /// The Network message id to identify the message
    /// </summary>
    public static short MSG_ID = 1002;
}
