using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;


public class GBuffer  : MonoBehaviour
{

    public RenderTexture[] gBufferTextures;
    public RenderBuffer[] rtBuffers;

    public int Width;
    public int Height;

    public Camera camera;




    /// <summary>
    /// 
    /// </summary>
    public RenderTexture NormalBufferTexture
    {
        get { return gBufferTextures[0]; }
        set { gBufferTextures[0] = value; }
    }




    /// <summary>
    /// 
    /// </summary>
    public RenderTexture AlbedoBufferTexture
    {
        get { return gBufferTextures[1]; }
        set { gBufferTextures[1] = value; }
    }




    /// <summary>
    /// 
    /// </summary>
    public RenderTexture SpecularBufferTexture
    {
        get { return gBufferTextures[2]; }
        set { gBufferTextures[2] = value; }
    }




    /// <summary>
    /// 
    /// </summary>
    public RenderTexture PositionBufferTexture
    {
        get { return gBufferTextures[3]; }
        set { gBufferTextures[3] = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public RenderBuffer DepthBuffer
    {
        get { return NormalBufferTexture.depthBuffer; }
        
    }


    /// <summary>
    /// 
    /// </summary>
    public void Create()
    {

        gBufferTextures = new RenderTexture[4];
        rtBuffers = new RenderBuffer[4];


        // Create a Render Texture for each color attachment
        // Normale Render Texture has additionally a depth buffer attached, therefore depth is 24
        for (int i = 0; i < gBufferTextures.Length; i++)
        {
            // Set depth on normalTexture, since it stores depth as well
            int depth = i == 0 ? 24 : 0;
            gBufferTextures[i] = CreateRenderTexture(Width, Height, depth, RenderTextureFormat.ARGBFloat);
            rtBuffers[i] = gBufferTextures[i].colorBuffer;
        }
    }




    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        gBufferTextures = null;
        rtBuffers = null;
    }




    /// <summary>
    /// 
    /// </summary>
    public static RenderTexture CreateRenderTexture(int width, int height, int depth, RenderTextureFormat format)
    {
        Debug.Log("DeferredRenderer.CreateRenderTexture() " + width + ", " + height + ", " + depth);
        RenderTexture r = new RenderTexture(width, height, depth, format);
        r.filterMode = FilterMode.Point;
        r.useMipMap = false;
        r.generateMips = false;
        r.enableRandomWrite = true;
        //r.wrapMode = TextureWrapMode.Repeat;
        r.Create();
        return r;
    }



    public void BindAsRenderTarget()
    {
        Graphics.SetRenderTarget(rtBuffers, NormalBufferTexture.depthBuffer);
    }
}
