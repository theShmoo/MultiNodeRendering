using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System.IO;

/// <summary>
/// The GBufferInterface works as an easy interface to quickly access the several color buffers and depth buffers from the default gBuffer for deferred Rendering. 
/// </summary>
[RequireComponent(typeof (Camera))]
public class GBufferInterface : MonoBehaviour
{
    private new Camera camera;

    CommandBuffer buffer;

    private RenderTexture albedo;
    private RenderTexture specular;
    private RenderTexture normals;
    private RenderTexture emission;
    private RenderTexture depth;




    /// <summary>
    /// Returns the albedo color buffer. 
    /// </summary>
    public RenderTexture Albedo
    {
        get { return albedo; }       
    }



    
    /// <summary>
    /// Returns the specular color buffer
    /// </summary>
    public RenderTexture Specular
    {
        get { return specular; }        
    }
    



    /// <summary>
    /// Returns the normal buffer
    /// </summary>
    public RenderTexture Normals
    {
        get { return normals; }
       
    }
    



    /// <summary>
    /// Returns the emission color buffer
    /// </summary>
    public RenderTexture Emission
    {
        get { return emission; }
        
    }
    



    /// <summary>
    /// Returns the depth buffer
    /// </summary>
    public RenderTexture Depth
    {
        get { return depth; }
        
    }




    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
   
        
        // Create Render Textures for gBuffer copy
        albedo = CreateRenderTexture(RenderTextureFormat.ARGB32);                                                     // Diffuse color
        specular = CreateRenderTexture(RenderTextureFormat.ARGB32);                                                   // Specular color
        normals = CreateRenderTexture(RenderTextureFormat.ARGB2101010);                                               // World space normal
        emission = CreateRenderTexture(Camera.main.hdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB2101010);  // Emission + lighting + lightmaüs + refletion probes
        depth = CreateRenderTexture(RenderTextureFormat.Depth, 16);                                                   // Depth


         // Create Commands to copy gBuffer content to the Render Textures
        buffer = new CommandBuffer();
        buffer.name = "Blit gBuffer";

        buffer.Blit(BuiltinRenderTextureType.GBuffer0, albedo);
        buffer.Blit(BuiltinRenderTextureType.GBuffer1, specular);
        buffer.Blit(BuiltinRenderTextureType.GBuffer2, normals);
        buffer.Blit(BuiltinRenderTextureType.GBuffer3, emission);
        //buffer.Blit(BuiltinRenderTextureType.Depth, depth);
        // Assign the Buffer to a camera
        camera = GetComponent<Camera>();
        camera.AddCommandBuffer(CameraEvent.BeforeLighting, buffer);
        
    }




    /// <summary>
    /// Creates a RenderTexture with the given parameter
    /// </summary>
    private RenderTexture CreateRenderTexture(RenderTextureFormat format, int depth = 0)
    {
        var texture = new RenderTexture(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, depth, format);
        texture.filterMode = FilterMode.Bilinear;
        texture.useMipMap = true;
        texture.generateMips = true;

        texture.Create();
        return texture;
    }
}
