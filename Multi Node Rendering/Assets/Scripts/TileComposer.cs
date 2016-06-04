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
    public List<Texture2D> renderedImages;

    public Material texturedQuadMaterial;

    TextureNetworkManager textureNetworkManager;

    private bool active = false;
    [SerializeField]
    [Range(0, 2)]
    public float opacity = 1;

    [SerializeField]
    [Range(0, 1)]
    public int pass = 0;

    public bool Active
    {
        get { return active; }
        set { active = value; }
    }

    public int Pass
    {
        get { return pass; }
        set {
            this.pass = value;
            OnRaycasterParameterChanged();
        }
    }
    public float Opacity
    {
        get { return opacity; }
        set
        {
            this.opacity = value;
            OnRaycasterParameterChanged();
        }
    }

    void OnRaycasterParameterChanged()
    {
        TextureNetworkManager.Instance.OnRaycasterParameterChanged(pass, opacity);
    }

    // Use this for initialization
	void Start () {
        tiles = new List<ScreenTile>();
        renderedImages = new List<Texture2D>();
        NumTilesChanged(numTiles);
	}
	
	// Update is called once per frame
	void Update () {
        // nothing to do
	}

    public void SetTexture(int iTileIndex, byte[] data)
    {
        var tex = TileNetworkManager.Instance.tileComposer.renderedImages[iTileIndex];
        tex.LoadImage(data);
        tex.Apply();
    }

    public void sendTextureToServer(int iTileIndex, byte[] textureData)
    {
        //byte error;
        //NetworkTransport.Send(socketId, connectionId, myReliableChannelId, textureData, textureData.Length, out error);
        //LogNetworkError(error);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (active)
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


        int screenWidth = 64;
        int screenHeight = 32;

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
                renderedImages.Add(tex);
            }
        }
    }

    /// <summary>
    /// send rpc calls to all clients and set which tile they are responsible for
    /// </summary>
    /// <param name="hostConnectionId"></param>
    public void ArrangeTilesToRaycaster(int hostConnectionId)
    {
//         int i = 0;
//         this.raycaster.Clear();
//         foreach (var conn in NetworkServer.connections)
//         {
//             // can be null for disconnected clients or the host
//             if (conn == null || conn.connectionId == hostConnectionId)
//                 continue;
// 
//             foreach (var player in conn.playerControllers)
//             {
//                 var r = player.gameObject.GetComponent<TileRaycaster>();
//                
//                 // send the tile index to the client
// //                 var msg = new TileIndexMessage();
// //                 msg.tileIndex = tiles[i].tileIndex;
// //                 msg.netId = r.netId;
// //                 conn.Send(TileIndexMessage.MSG_ID, msg);
// 
//                 r.RpcSetTile(tiles[i]);
//                 this.raycaster.Add(r);
//                 i++;
//                 if (i >= tiles.Count)
//                     break;
//             }
//             if (i >= tiles.Count)
//                 break;
//         }
        
    }
}
