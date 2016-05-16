using UnityEngine;
using System.Collections.Generic;

public class TileComposer : MonoBehaviour
{

    private Vector2 numTiles = new Vector2(2,2);

    private List<ScreenTile> tiles;
    public List<Texture2D> renderedImages;

    public Material texturedQuadMaterial;

    // Use this for initialization
	void Start () {
        tiles = new List<ScreenTile>();
        renderedImages = new List<Texture2D>();
        NumTilesChanged(numTiles);
	}
	
	// Update is called once per frame
	void Update () {
        TileRaycaster rayCaster = Camera.main.GetComponent<TileRaycaster>();

        // Set current state to TileRaycaster
        RayCastState state = new RayCastState();
        state.volumeWorldMatrix = Matrix4x4.identity;
        state.viewMatrix = Camera.main.worldToCameraMatrix;
        state.projectionMatrix = Camera.main.projectionMatrix;

        rayCaster.SetSceneState(state);

        // Render each tile
        for (int i = 0; i < tiles.Count; i++)
        {
            ScreenTile tile = tiles[i];

            rayCaster.SetTile(tile);
            rayCaster.RenderTile();

            int width = (int)(Screen.width / numTiles.x);
            int height = (int)(Screen.height / numTiles.y);

            // Store content in Texture2D
            Graphics.SetRenderTarget(rayCaster.GetRenderedImage());

            Texture2D img = renderedImages[i];
            img.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            img.Apply();
        }
	}

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.SetRenderTarget(dest);
        GL.Clear(true, true, Color.black);

        for (int i = 0; i < tiles.Count; i++ )
        {
            Vector2 tileIndex = tiles[i].tileIndex;
            Vector2 numTiles = tiles[i].numTiles;
            Texture2D image = renderedImages[i];

            
            // Create Scale and translation Matrix
            Vector3 scale = new Vector3(1.0f / numTiles.x, 1.0f / numTiles.y, 1.0f);
            Vector3 translate = new Vector3((tileIndex.x / (numTiles.x - 1.0f) - 0.5f) * -1.0f, 0.0f, 0.0f);
            
            Matrix4x4 M = Matrix4x4.TRS(translate, Quaternion.identity, scale);
            
            texturedQuadMaterial.SetTexture("_MainTex", image);
            texturedQuadMaterial.SetPass(0);

            // Scale viewport rect to the tile position
            float sl = 1.0f - (2.0f * tileIndex.x / numTiles.x);
            float sr = -(sl - 2.0f / numTiles.x);
            float sb = 1.0f - (2.0f * tileIndex.y / numTiles.y);
            float st = -(sb - 2.0f / numTiles.y);

            float left   =  -1 * sl;
            float right  = 1 * sr;
            float bottom = -1 * sb;
            float top   = 1 * st;



            GL.Begin(GL.QUADS);
            {
                //GL.TexCoord2(0.0f, 0.0f);
                //GL.Vertex3(-left, -1.0f, 1.0f);

                //GL.TexCoord2(1.0f, 0.0f);
                //GL.Vertex3(1.0f, -1.0f, 1.0f);

                //GL.TexCoord2(1.0f, 1.0f);
                //GL.Vertex3(1.0f, 1.0f, 1.0f);

                //GL.TexCoord2(0.0f, 1.0f);
                //GL.Vertex3(-1.0f, 1.0f, 1.0f);

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
}
