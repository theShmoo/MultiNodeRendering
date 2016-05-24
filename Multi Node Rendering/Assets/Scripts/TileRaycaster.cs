using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

/// <summary>
/// This class represents a state structure for the scene/animation for distributed rendering systems to transmit all neccessary parameters to the render node
/// </summary>
public class RayCastState
{
    public long deltaTime;

    public Matrix4x4 viewMatrix;

    public Matrix4x4 projectionMatrix;

    public Matrix4x4 volumeWorldMatrix;
}

/// <summary>
/// Performs raycast rendering of the scene to the given tile
/// </summary>
public class TileRaycaster : NetworkBehaviour
{

    // A collection to store all tiles to be rendered
    private ScreenTile tile;

    // A collection to store all rendered images
    private Texture2D tileImage;

    // State object
    private RayCastState state;
    private bool stateChanged;

    private Vector2 numTiles;


    [SerializeField]
    private Texture3D volumeTexture;
    [SerializeField]
    private Mesh boundingBox;
    [SerializeField]
    private Material depthPassMaterial;
    [SerializeField]
    private Material rayMarchMaterial;
    
    public Material texturedQuadMaterial;

    [SerializeField]
    private Texture2D renderedImage;

    private float opacity = 1;

    private int pass = 0;

    private Vector2 renderTileIndex = new Vector2 (-1,-1);

    public int Pass
    {
        get { return pass; }
        set
        {
            this.pass = value;
        }
    }
    public float Opacity
    {
        get { return opacity; }
        set
        {
            this.opacity = value;
        }
    }

    private bool tileDimensionsChanged = true;

    [SerializeField]
    private RenderTexture compositeBuffer;
    [SerializeField]
    private RenderTexture frontDepth;
    [SerializeField]
    private RenderTexture backDepth;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tile"></param>
    [ClientRpc]
    public void RpcSetTile(ScreenTile tile)
    {
        if(this.tile == null || this.tile.numTiles.x != tile.numTiles.x || this.tile.numTiles.y != tile.numTiles.y)
        {
            tileDimensionsChanged = true;
            Debug.Log("Dimensions Changed");
        }

        int width = (int)(tile.Size.x * tile.screenWidth);
        int height = (int)(tile.Size.y * tile.screenHeight);
        renderedImage = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        renderedImage.wrapMode = TextureWrapMode.Clamp;
        this.tile = tile;
    }

    /// <summary>
    /// Set the tile index this client should render. 
    /// </summary>
    /// <param name="tileIndex">a Vector2 defining the tile index</param>
    [ClientRpc]
    public void RpcSetRenderedTileIndex(Vector2 tileIndex)
    {
        this.renderTileIndex = tileIndex;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    [ClientRpc]
    public void RpcSetSceneState(RayCastState state)
    {
        // Set new state
        this.state = state;
        stateChanged = true;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Texture2D GetRenderedImage()
    {
        return renderedImage;
    }

    
    /// <summary>
    /// 
    /// </summary>
    private void OnPostRender()
    {
        if (stateChanged)
        {
            //RenderTile();
            stateChanged = false;
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (isServer || tile == null || renderTileIndex != tile.tileIndex)
        {
            return;
        }

        Graphics.SetRenderTarget(dest);
        GL.Clear(true, true, Color.black);

        texturedQuadMaterial.SetTexture("_MainTex", renderedImage);
        texturedQuadMaterial.SetPass(0);

        // Scale viewport rect to the tile position
        float sl = 1.0f - (2.0f * tile.tileIndex.x / tile.numTiles.x);
        float sr = -(sl - 2.0f / tile.numTiles.x);
        float sb = 1.0f - (2.0f * tile.tileIndex.y / tile.numTiles.y);
        float st = -(sb - 2.0f / tile.numTiles.y);

        float left = -1 * sl;
        float right = 1 * sr;
        float bottom = -1 * sb;
        float top = 1 * st;

        GL.Begin(GL.QUADS);
        {
            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex3(left, bottom, 0.0f);

            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex3(right, bottom, 0.0f);

            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex3(right, top, 0.0f);

            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex3(left, top, 0.0f);
        }
        GL.End();
    }

    /// <summary>
    /// 
    /// </summary>
    [ClientRpc]
    public void RpcRenderTile()
    {
        if (isServer || tile == null || renderTileIndex != tile.tileIndex)
        {
            return;
        }

        // Size of the tile in px      
        int width = (int)(tile.Size.x * tile.screenWidth);
        int height = (int)(tile.Size.y * tile.screenHeight);

        // Create new Render Textures if the size of a tile changes
        if (tileDimensionsChanged)
        {
            backDepth       = CreateRenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            frontDepth      = CreateRenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            compositeBuffer = CreateRenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);

            tileDimensionsChanged = false;
        }

        Matrix4x4 P = tile.getOffCenterProjectionMatrix();
        Matrix4x4 V = state.viewMatrix;
        Matrix4x4 M = state.volumeWorldMatrix;
        // Draw front faces of bounding box to texture      
        Graphics.SetRenderTarget(frontDepth);
        GL.Clear(true, true, Color.black);

        depthPassMaterial.SetMatrix("_MVPMatrix", P * V * M);
        depthPassMaterial.SetPass(0);
        Graphics.DrawMeshNow(boundingBox, state.volumeWorldMatrix, 0);

        // Draw back faces of bounding box to texture
        Graphics.SetRenderTarget(backDepth);
        GL.Clear(true, true, Color.black);

        depthPassMaterial.SetPass(1);
        Graphics.DrawMeshNow(boundingBox, state.volumeWorldMatrix, 0);

        // Perform Raycasting in a full screen pass
        Graphics.SetRenderTarget(compositeBuffer);
        GL.Clear(true, true, Color.black);
            
        rayMarchMaterial.SetFloat("_Opacity", opacity); // Blending strength 
        rayMarchMaterial.SetTexture("_BackTex", backDepth);
        rayMarchMaterial.SetTexture("_FrontTex", frontDepth);
        rayMarchMaterial.SetTexture("_VolumeTex", volumeTexture);
        rayMarchMaterial.SetVector("_TextureSize", new Vector4(volumeTexture.width, volumeTexture.height, volumeTexture.depth));

        Graphics.Blit(null, compositeBuffer, rayMarchMaterial, pass);

        renderedImage.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
        renderedImage.Apply();
                   
    }

    void Update()
    {
        //RenderTile();
    }

    /// <summary>
    /// 
    /// </summary>
    public static RenderTexture CreateRenderTexture(int width, int height, int depth, RenderTextureFormat format)
    {
        //Debug.Log("DeferredRenderer.CreateRenderTexture() " + width + ", " + height + ", " + depth);
        RenderTexture r = new RenderTexture(width, height, depth, format);
        r.filterMode = FilterMode.Point;
        r.useMipMap = false;
        r.generateMips = false;
        r.enableRandomWrite = true;
        //r.wrapMode = TextureWrapMode.Repeat;
        r.Create();
        return r;
    }
}
