using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;


[ExecuteInEditMode]
public class TestBehaviour : MonoBehaviour
{


    public Mesh mesh;
    public Material mat;

    private CommandBuffer buffer;
    private CommandBuffer buffer2;



    void Start()
    {
        DeferredRenderer renderer = Camera.main.GetComponent<DeferredRenderer>();
        renderer.AddRenderCallback(DeferredRenderer.RenderEvent.GBUFFER, Render);
    }

    void OnWillRenderObject()
    {
        

    }

    void OnRenderObject()
    {
      //  mat.SetPass(0);
       // Graphics.DrawMeshNow(mesh, this.transform.localToWorldMatrix);

    }


    // Update is called once per frame
    void Update()
    {
       // Camera.main.Render();
        this.gameObject.transform.Rotate(Vector3.up, 90.0f * Time.deltaTime);
        
    }

    public void Render()
    {
        mat.SetPass(0);
        Graphics.DrawMeshNow(mesh, this.transform.position, this.transform.rotation);
    }
}
