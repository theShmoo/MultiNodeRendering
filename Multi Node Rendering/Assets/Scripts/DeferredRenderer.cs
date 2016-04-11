using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class DeferredRenderer : MonoBehaviour
{

    /// <summary>
    /// Geometry Buffer
    /// </summary>
    public GBuffer gBuffer;
  
    /// <summary>
    /// RenderTexture to store the (deferred) rendered image before swapping it to the screen
    /// </summary>
    public RenderTexture compositeBuffer;

    /// <summary>
    /// Material used to render a textured full screen quad
    /// </summary>
    public Material fullScreenQuadMaterial;

    /// <summary>
    /// material used to render the deferred Geometry Pass
    /// </summary>
    public Material deferredGeometryPassMaterial;

    /// <summary>
    /// Material used to render the deferred Lighting Pass
    /// </summary>
    public Material deferredLightingPassMaterial;

    // Remove me when callback rendering system is in place
    private TestBehaviour cube;

    private Camera camera;

    private bool active = false;


    public bool Active
    {
        get { return active; }
        set { active = value; }
    }


    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        gBuffer = GetComponent<GBuffer>();
        if(!gBuffer)
        {
           gBuffer = this.gameObject.AddComponent<GBuffer>();
        }

        camera = GetComponent<Camera>();

        // Create a Render Texture for composing the image
        compositeBuffer = GBuffer.CreateRenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGBFloat);
        compositeBuffer.filterMode = FilterMode.Trilinear;
        
    }



    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {

    }



    /// <summary>
    ///  Use this for initialization
    /// </summary>

    void Start()
    {
       
       
    }




    /// <summary>
    /// Render custom deferred Pipeline after after the camera has rendered its part
    /// </summary>
    void OnPostRender()
    {
        // Don't render if not active
        if (!active) return;

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

        gBuffer.BindAsRenderTarget();
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
        DrawFullscreenQuad(gBuffer.AlbedoBufferTexture);
    }




    /// <summary>
    /// Applies Postprocess effects to the rendered image
    /// </summary>
    private void RenderPostprocess()
    {
        // TODO 
    }




    /// <summary>
    /// Draws a textured quad in full screen. 
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
