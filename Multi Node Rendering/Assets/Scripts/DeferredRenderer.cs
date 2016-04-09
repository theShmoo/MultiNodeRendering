using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
public class DeferredRenderer : MonoBehaviour
{

    public RenderTexture[] gBufferTextures;
    public RenderBuffer[] gBuffer;

    private RenderTexture compositeBuffer;

    public Camera camera;

    /// <summary>
    /// 
    /// </summary>
    public Material fullScreenQuadMaterial;

    /// <summary>
    /// 
    /// </summary>
    public Material deferredGeometryPassMaterial;




    private TestBehaviour cube;


    /// <summary>
    /// 
    /// </summary>
    public RenderTexture NormalBufferTexture
    {
        get { return gBufferTextures[0]; }
    }



    /// <summary>
    /// 
    /// </summary>
    public RenderTexture AlbedoBufferTexture
    {
        get { return gBufferTextures[1]; }
    }



    /// <summary>
    /// 
    /// </summary>
    public RenderTexture SpecularBufferTexture
    {
        get { return gBufferTextures[2]; }
    }



    /// <summary>
    /// 
    /// </summary>
    public RenderTexture PositionBufferTexture
    {
        get { return gBufferTextures[3]; }
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

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        gBufferTextures = new RenderTexture[4];
        gBuffer = new RenderBuffer[4];

        camera = GetComponent<Camera>();

        CreateRenderTargets(camera.pixelWidth, camera.pixelHeight);
    }



    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        gBufferTextures = null;
        gBuffer = null;
    }



    /// <summary>
    ///  Use this for initialization
    /// </summary>

    void Start()
    {
       
       
    }




    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {

    }




    /// <summary>
    /// 
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    void CreateRenderTargets(int width, int height)
    {
        RenderTextureFormat format = RenderTextureFormat.ARGBFloat;
        // Create a Render Texture for each color attachment
        
        for (int i = 0; i < gBuffer.Length; i++)
        {
            // Set depth on normalTexture, since it stores depth as well
            int depth = i == 0 ? 32 : 0;
            gBufferTextures[i] = CreateRenderTexture(width, height, depth, format);
            gBuffer[i] = gBufferTextures[i].colorBuffer;
        }

        // Create a Render Texture for composing the image

        compositeBuffer = CreateRenderTexture(width, height, 0, format);
        compositeBuffer.filterMode = FilterMode.Trilinear;
    }




    /// <summary>
    /// Render custom deferred Pipeline after after the camera has rendered its part
    /// </summary>
    void OnPostRender()
    {
        // Store the current RenderTarget
        RenderTexture current = RenderTexture.active;

        // Do Deferred Geometry Pass
        RenderGeometryPass();

        // Do Deferred Lighting Pass
        RenderLightingPass();

        // Set the stored RenderTexture as RenderTarget
        
        RenderPostprocess();


        // Swap Image to screen
        Graphics.SetRenderTarget(current);        
        DrawFullscreenQuad(compositeBuffer);
    }




    /// <summary>
    /// Performs the geometry pass for deferred rendering
    /// </summary>
    private void RenderGeometryPass()
    {
        
        Graphics.SetRenderTarget(gBuffer, NormalBufferTexture.depthBuffer);
        GL.Clear(true, true, Color.black);
        // TODO: Render all objects here using referenced callback functions

        // Dummy Solution
         GameObject.Find("Cube").GetComponent<TestBehaviour>().Render();
    }




    /// <summary>
    /// Performs the lighting pass for deferred rendering and renders it into the composite buffer texture
    /// </summary>
    private void RenderLightingPass()
    {
        Graphics.SetRenderTarget(compositeBuffer);
        GL.Clear(true, true, Color.black);
        // TODO Render all Light sources
       
        // Change this: This currently only renders the albedo texture unto the compositeBUffer     
        DrawFullscreenQuad(AlbedoBufferTexture);
    }




    /// <summary>
    /// Applies Postprocess effects to the rendered image
    /// </summary>
    private void RenderPostprocess()
    {
        // TODO 
    }




    /// <summary>
    /// Draws a quad in full screen. 
    /// Use this rather then drawing a mesh quad in the origin, since View Frustum Culling would cull a mesh object
    /// </summary>
    /// <param name="z"></param>
    public void DrawFullscreenQuad(Texture texture)
    {
        fullScreenQuadMaterial.SetPass(0);
        fullScreenQuadMaterial.SetTexture("_MainTex", texture);

        GL.Begin(GL.QUADS);
        {
            GL.Vertex3(-1.0f, -1.0f, 1.0f);
            GL.Vertex3(1.0f, -1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 1.0f);
            GL.Vertex3(-1.0f, 1.0f, 1.0f);

            //GL.Vertex3(-1.0f, 1.0f, 1.0f);
            //GL.Vertex3(1.0f, 1.0f, 1.0f);
            //GL.Vertex3(1.0f, -1.0f, 1.0f);
            //GL.Vertex3(-1.0f, -1.0f, 1.0f);
        }
        GL.End();

    }
}
