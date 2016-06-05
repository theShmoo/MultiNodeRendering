using UnityEngine;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(TileComposer))]
public class CustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (target.GetType() == typeof(TileComposer))
        {
            TileComposer getterSetter = (TileComposer)target;
            getterSetter.Pass = getterSetter.pass;
            getterSetter.Opacity = getterSetter.opacity;
        }
    }
}
#endif

public class TileComposer : MonoBehaviour
{

    private Vector2 numTiles = new Vector2(0,0);

    public List<ScreenTile> tiles;
    public Dictionary<Vector2,Texture2D> renderedImages;

    public Material texturedQuadMaterial;

    TextureNetworkManager textureNetworkManager;

    private bool active = false;
    [SerializeField]
    [Range(0, 2)]
    public float opacity = 1;

    private float lastOpacity = 1;

    [SerializeField]
    [Range(0, 1)]
    public int pass = 0;

    private int lastPass = 0;

    public bool Active
    {
        get { return active; }
        set { active = value; }
    }

    public int Pass
    {
        get { return pass; }
        set {
            if(this.lastPass != value)
            {
                this.lastPass = this.pass;
                this.pass = value;
                OnRaycasterParameterChanged();
            }
        }
    }
    public float Opacity
    {
        get { return opacity; }
        set
        {
            if (this.lastOpacity != value)
            {
                this.lastOpacity = this.opacity;
                this.opacity = value;
                OnRaycasterParameterChanged();
            }
        }
    }

    void OnRaycasterParameterChanged()
    {
        if (TextureNetworkManager.Instance != null)
            TextureNetworkManager.Instance.OnRaycasterParameterChanged(pass, opacity);
    }

    // Use this for initialization
	void Start () {
        tiles = new List<ScreenTile>();
        renderedImages = new Dictionary<Vector2, Texture2D>();
        NumTilesChanged(numTiles);
	}
	
	// Update is called once per frame
	void Update () {
        // nothing to do
	}

    public void SetTexture(Vector2 tileIndex, byte[] data)
    {
        var tex = renderedImages[tileIndex];
        tex.LoadImage(data);
        tex.Apply();
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (active)
        {
            Graphics.SetRenderTarget(dest);
            GL.Clear(true, true, Color.black);

            if (tiles == null) return;

            foreach (var tile in tiles)
            {
                Vector2 tileIndex = tile.tileIndex;
                Vector2 numTiles = tile.numTiles;
                Texture2D image = renderedImages[tileIndex];

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
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void NumTilesChanged(Vector2 numTiles)
    {
        this.numTiles = numTiles;

        // Dispose old tiles
        tiles.Clear();
        // Dispose old textures
        renderedImages.Clear();


        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        int width = (int)(screenWidth / numTiles.x);
        int height = (int)(screenHeight / numTiles.y);
        
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

                tile.screenWidth = screenWidth;
                tile.screenHeight = screenHeight;
                
                tiles.Add(tile);
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                renderedImages.Add(tile.tileIndex,tex);
            }
        }
    }
}
