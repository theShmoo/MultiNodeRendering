using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class DeferredRenderer : MonoBehaviour
{
    private PerspectiveTile screenTile;


    /// <summary>
    /// A RenderEvent describes a state of the renderer. Use this to insert custom render functionality into the render pipeline at this state
    /// <param name="GBUFFER">GBUFFER: 
    /// Renders the geometry pass into a set up gBuffer
    /// Before this pass, the gBuffer is empty. 
    /// </param>
    /// <param name="LIGHTING">LIGHTING: 
    /// Renders lighting information into the composite buffer, gBuffer is set up as textures. In order to convolute lighting correctly it is recommended to use additive blending.
    /// Before this pass, the composite buffer is empty. 
    /// </param>
    /// <param name="TRANSPARENT">TRANSPARENT: 
    /// Execute custom transparent render functions as a forward pass. 
    /// The composite buffer is up as render target and the depth buffer has beeen blit into it, to use correct depth testing
    /// </param>
    /// <param name="EFFECT">EFFECT: 
    /// The results of Lighting und forward pass stored in the composite buffer are accessible via textures in order to allow screen space effects. In order to convolute the effects correctly it is recommended to use additive blending and disable depth test.
    /// NOT YET SUPPORTED!!
    /// </param>
    /// </summary>
    public enum RenderEvent
    {
        GBUFFER = 0, LIGHTING = 1, TRANSPARENT = 2, EFFECT = 3
    };

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

    //--------------------------------------------------------------------------------------
    // Collections to store all callbacks
    //--------------------------------------------------------------------------------------
    private List<Action> gBufferCallbacks = new List<Action>();
    private List<Action> lightingCallbacks = new List<Action>();
    private List<Action> transparentCallbacks = new List<Action>();
    private List<Action> effectCallbacks = new List<Action>();

    /// <summary>
    /// Adds a custom render callback to the renderer at the given render event. 
    /// Use this to dispatch custom render code. 
    /// </summary>
    public void AddRenderCallback(RenderEvent ev, Action callback)
    {
        switch (ev)
        {
            case RenderEvent.GBUFFER:
                gBufferCallbacks.Add(callback);
                break;
            case RenderEvent.LIGHTING:
                lightingCallbacks.Add(callback);
                break;
            case RenderEvent.TRANSPARENT:
                transparentCallbacks.Add(callback);
                break;
            case RenderEvent.EFFECT:
                effectCallbacks.Add(callback);
                break;
        }
    }

    public bool Active
    {
        get { return active; }
        set { active = value; }
    }



    /// <summary>
    /// Sets the tile of the screen to render
    /// </summary>
    /// <param name="screenTile"></param>
    public void SetScreenTile(PerspectiveTile screenTile)
    {
        this.screenTile = screenTile;
        gBuffer = this.gameObject.AddComponent<GBuffer>();
        // Create gBuffer and composite buffer according to the size of the screen tile
        gBuffer.Width = (int)(screenTile.Size.x * (float)Camera.main.pixelWidth);
        gBuffer.Height = (int)(screenTile.Size.y * (float)Camera.main.pixelHeight);
        
       
        gBuffer.Create();
        compositeBuffer = GBuffer.CreateRenderTexture(gBuffer.Width, gBuffer.Height, 0, RenderTextureFormat.ARGBFloat);
        compositeBuffer.filterMode = FilterMode.Trilinear;
    }



    void LateUpdate()
    {
        if (screenTile == null)
            return;
        Camera.main.projectionMatrix = screenTile.getOffCenterProjectionMatrix();
    }



    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {        
        camera = GetComponent<Camera>();
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
    /// Render custom deferred Pipeline after the camera has rendered its part
    /// </summary>
    void OnPostRender()
    {
        // Don't render if not active
        if (!active || screenTile == null) return;

        // Store the current RenderTarget
        RenderTexture current = RenderTexture.active;

        // Do Deferred Geometry Pass
        RenderGeometryPass();

        // Do Deferred Lighting Pass
        RenderLightingPass();

        // Set the stored RenderTexture as RenderTarget

        RenderPostprocess();


        // Lastly swap image to screen
        Graphics.SetRenderTarget(current);
        //DrawFullscreenQuad(compositeBuffer);
        fullScreenQuadMaterial.SetTexture("_MainTex", compositeBuffer);
        Graphics.Blit(null, current, fullScreenQuadMaterial, 0);
    }




    /// <summary>
    /// Performs the geometry pass for deferred rendering
    /// </summary>
    private void RenderGeometryPass()
    {

        gBuffer.BindAsRenderTarget();
        GL.Clear(true, true, Color.black);

        // Execute all gBuffer callbacks
        foreach (Action cb in gBufferCallbacks)
        {
            cb.Invoke();
        }
    }




    /// <summary>
    /// Performs the lighting pass for deferred rendering and renders it into the composite buffer texture
    /// </summary>
    private void RenderLightingPass()
    {
        // Bind and clear the composite buffer
        Graphics.SetRenderTarget(compositeBuffer);
        GL.Clear(true, true, Color.black);

        // TODO Render all Light sources
        // Change this: This currently only renders the albedo texture unto the compositeBuffer     

        // Execute all lighting callbacks ( TO execute custom lighting (Using additive blending)
        foreach (Action cb in lightingCallbacks)
        {
            cb.Invoke();
        }

        //DrawFullscreenQuad(gBuffer.AlbedoBufferTexture);

        //fullScreenQuadMaterial.SetTexture("_MainTex", gBuffer.AlbedoBufferTexture);
        //Graphics.Blit(null, compositeBuffer, fullScreenQuadMaterial, 0);
        // TODO: Blit Depth buffer into the composite buffer in order to allow transparent objects to be rendered using forward rendering
        // Graphics.Blit(this.gBuffer.DepthBuffer, compositeBuffer.depthBuffer);

        // Execute all transparent callbacks to render transparent obejcts in forward pass (Using depth test)
        foreach (Action cb in transparentCallbacks)
        {
            cb.Invoke();
        }
    }




    /// <summary>
    /// Applies Postprocess effects to the rendered image
    /// </summary>
    private void RenderPostprocess()
    {
        foreach (Action cb in effectCallbacks)
        {
            cb.Invoke();
        }
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
