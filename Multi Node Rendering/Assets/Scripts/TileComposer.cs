using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class TileComposer : NetworkBehaviour
{

    private Vector2 numTiles = new Vector2(0,0);

    private List<ScreenTile> tiles;
    private List<TileRaycaster> raycaster = null;
    public List<Texture2D> renderedImages;

    public Material texturedQuadMaterial;

    private bool active = false;

    public bool Active
    {
        get { return active; }
        set { active = value; }
    }

    // Use this for initialization
	void Start () {
        tiles = new List<ScreenTile>();
        renderedImages = new List<Texture2D>();
        NumTilesChanged(numTiles);
	}
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer || raycaster == null || !active)
        {
            // the server does nothing
            return;
        }
        
        // Set current state to TileRaycaster
        RayCastState state = new RayCastState();
        state.volumeWorldMatrix = Matrix4x4.identity;
        state.viewMatrix = Camera.main.worldToCameraMatrix;
        state.projectionMatrix = Camera.main.projectionMatrix;
        int width = (int)(Screen.width / numTiles.x);
        int height = (int)(Screen.height / numTiles.y);

        for (int i = 0; i < tiles.Count; i++)
        {
            raycaster[i].SetSceneState(state);
            raycaster[i].RpcRenderTile();
            renderedImages[i] = raycaster[i].GetRenderedImage();
        }

        // Render each tile
//         for (int i = 0; i < tiles.Count; i++)
//         {
//             ScreenTile tile = tiles[i];
// 
//             rayCaster.SetTile(tile);
//             rayCaster.RenderTile();
// 
//             int width = (int)(Screen.width / numTiles.x);
//             int height = (int)(Screen.height / numTiles.y);
// 
//             // Store content in Texture2D
//             Graphics.SetRenderTarget(rayCaster.GetRenderedImage());
// 
//             Texture2D img = renderedImages[i];
//             img.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
//             img.Apply();
//         }
        
	}

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (!isLocalPlayer || !active) // todo change
        {
            // the client does nothing
            return;
        }
        else
        {

            Graphics.SetRenderTarget(dest);
            GL.Clear(true, true, Color.black);

            if (tiles == null) return;

            for (int i = 0; i < tiles.Count; i++)
            {
                Vector2 tileIndex = tiles[i].tileIndex;
                Vector2 numTiles = tiles[i].numTiles;
                Texture2D image = renderedImages[i];

                texturedQuadMaterial.SetTexture("_MainTex", image);
                texturedQuadMaterial.SetPass(0);

                // Scale viewport rect to the tile position
                float sl = 1.0f - (2.0f * tileIndex.x / numTiles.x);
                float sr = -(sl - 2.0f / numTiles.x);
                float sb = 1.0f - (2.0f * tileIndex.y / numTiles.y);
                float st = -(sb - 2.0f / numTiles.y);

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
                //break;
            }

            // Graphics.Blit(src, dest);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="dimX"></param>
    /// <param name="dimY"></param>
    public void NumTilesChanged(Vector2 numTiles)
    {
        this.numTiles = numTiles;

        // Dispose old tiles
        tiles.Clear();
        // Dispose old textures
        renderedImages.Clear();

        
        int width  = (int)(Screen.width / numTiles.x);
        int height = (int)(Screen.height / numTiles.y);
        
        // Create new tile objects
        for (int i = 0; i < numTiles.x; i++)
        {
            for (int j = 0; j < numTiles.y; j++)
            {

                ScreenTile tile = new ScreenTile();
                tile.tileIndex = new Vector2(i, j);
                tile.numTiles = new Vector2(numTiles.x, numTiles.y);

                tile.fov = Camera.main.fieldOfView;
                tile.np = Camera.main.nearClipPlane;
                tile.fp = Camera.main.farClipPlane;
                tile.aspect = Camera.main.aspect;

                tile.screenWidth = Screen.width;
                tile.screenHeight = Screen.height;
                

                
                tiles.Add(tile);
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                renderedImages.Add(tex);
            }
        }
    }

    public void ArrangeTilesToRaycaster(List<TileRaycaster> raycaster)
    {
        int i = 0;
        foreach(var r in raycaster)
        {
            r.SetTile(tiles[i]);
            i++;
            if (i >= tiles.Count)
                break;
        }
        this.raycaster = raycaster;
    }

}
