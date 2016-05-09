using UnityEngine;
using System.Collections.Generic;


public class RayCastState : StateObject
{
    public Matrix4x4 viewMatrix;
    public Matrix4x4 projectionMatrix;

    public Matrix4x4 volumeWorldMatrix;
}

public class TileRaycaster : MonoBehaviour
{

    // A collection to store all tiles to be rendered
    private PerspectiveTile tile;

    // A collection to store all rendered images
    private Texture2D tileImage;

    // State object
    private RayCastState state;

    private Vector2 numTiles;


    [SerializeField]
    private Texture3D volumeTexture;
    [SerializeField]
    private Mesh boundingBox;
    [SerializeField]
    private Material depthPassMaterial;
    [SerializeField]
    private Material rayMarchMaterial;

   

    [SerializeField]
    private Texture2D renderedImage;

    [Range(0, 2)]
    public float opacity = 1;

    [Range(0, 1)]
    public int pass = 0;

    private bool tileDimensionsChanged = true;
    private bool stateChanged = true;

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
    public void SetTile(PerspectiveTile tile)
    {
        if(this.tile == null || this.tile.numTiles.x != tile.numTiles.x || this.tile.numTiles.y != tile.numTiles.y)
        {
            tileDimensionsChanged = true;
            Debug.Log("Dimensions Changed");
        }
       
        this.tile = tile;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    public void SetSceneState(RayCastState state)
    {
        // Set new state
        this.state = state;
        stateChanged = true;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public RenderTexture GetRenderedImage()
    {
        return compositeBuffer;
    }

    
    /// <summary>
    /// 
    /// </summary>
    private void OnPostRender()
    {
        //if (stateChanged)
        //{
        //    RenderTiles();           
        //    stateChanged = false;
        //}
    }

    /// <summary>
    /// 
    /// </summary>
    public void RenderTile()
    {      
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
                           
        }

    void Update()
    {
        RenderTile();
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
