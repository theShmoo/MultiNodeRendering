using UnityEngine;
using System.Collections.Generic;

public class TileComposer : MonoBehaviour
{

    private Vector2 numTiles = new Vector2(2, 2);

    private List<ScreenTile> tiles;  
    private Dictionary<Vector2, Texture2D> tileImages;

    public Texture2D[] imageArray;
	// Use this for initialization
	void Start () {
        tiles = new List<ScreenTile>();
        tileImages = new Dictionary<Vector2, Texture2D>();

        NumTilesChanged(numTiles);
	}
	
	// Update is called once per frame
	void Update () {

        TileRaycaster rayCaster = Camera.main.GetComponent<TileRaycaster>();

        // Set current state to TileRaycaster
        RayCastState state = new RayCastState();
        state.volumeWorldMatrix = this.transform.localToWorldMatrix;
        state.viewMatrix = Camera.main.worldToCameraMatrix;
        state.projectionMatrix = Camera.main.projectionMatrix;

        rayCaster.SetSceneState(state);

        imageArray = new Texture2D[tiles.Count];
        int idx = 0;
        foreach (var tile in tiles)
        {

            rayCaster.SetTile(tile);
            rayCaster.RenderTile();

            int width  = (int)(Screen.width / numTiles.x);
            int height = (int)(Screen.height / numTiles.y);

            Texture2D img = tileImages[tile.tileIndex];
            Graphics.SetRenderTarget(rayCaster.GetRenderedImage());
            img.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            
            img.Apply();
            imageArray[idx] = img;
            idx++;
        }

       
        
        //tileImages = rayCaster.GetRenderedImages();
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
        tileImages.Clear();

        
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

                tile.screenHeight = Screen.width;
                tile.screenWidth = Screen.height;

                tiles.Add(tile);
                // Create a dictionary entry for the tile
                tileImages[tile.tileIndex] = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            }
        }
    }
}
