using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

[ExecuteInEditMode]
public class LightBehaviour : MonoBehaviour
{

    /// <summary>
    /// Lightning pass shader
    /// </summary>
    public Material mat;

    // Use this for initialization
    void Start()
    {
        DeferredRenderer renderer = Camera.main.GetComponent<DeferredRenderer>();
        renderer.AddRenderCallback(DeferredRenderer.RenderEvent.LIGHTING, Render);
    }

    void OnWillRenderObject()
    {
    }

    void OnRenderObject()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Render()
    {
        DeferredRenderer renderer = Camera.main.GetComponent<DeferredRenderer>();
        GBuffer gBuffer = renderer.GetComponent<GBuffer>();
        if (!gBuffer)
        {
            Debug.LogError("No gBuffer object!");
        }

        mat.SetPass(0);
        mat.SetTexture("_WorldPositionMap", gBuffer.PositionBufferTexture);
        mat.SetTexture("_ColorMap", gBuffer.AlbedoBufferTexture);
        mat.SetTexture("_SpecularMap", gBuffer.SpecularBufferTexture);
        mat.SetTexture("_NormalMap", gBuffer.NormalBufferTexture);

        GL.Begin(GL.QUADS);
        {
            GL.Vertex3(-1.0f, -1.0f, 1.0f);
            GL.Vertex3(1.0f, -1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 1.0f);
            GL.Vertex3(-1.0f, 1.0f, 1.0f);
        }
        GL.End();
    }
}
