using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;


[ExecuteInEditMode]
public class TestBehaviour : MonoBehaviour
{


    public Mesh mesh;
    public Material mat;

    private CommandBuffer buffer;



    void Start()
    {
        buffer = new CommandBuffer();

        Camera.main.AddCommandBuffer(CameraEvent.BeforeGBuffer, buffer);
    }

    void OnWillRenderObject()
    {
      //  buffer.Clear();
      //  buffer.DrawMesh(mesh, this.transform.localToWorldMatrix, mat, 0, 0);

    }

    void OnRenderObject()
    {
     //   mat.SetPass(0);
     //   Graphics.DrawMeshNow(mesh, this.transform.localToWorldMatrix);

    }


    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.Rotate(Vector3.up, 90.0f * Time.deltaTime);      
    }
}
