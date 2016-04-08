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
        buffer = new CommandBuffer();
        buffer.name = "Draw GeometryCube";
        //Camera.main.AddCommandBuffer(CameraEvent.BeforeGBuffer, buffer);
      //  Camera.main.enabled = false;
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
        ////Debug.Log("bla");
        //buffer.Clear();
        //buffer.DrawMesh(mesh, this.transform.localToWorldMatrix, mat);
        Graphics.DrawMesh(mesh, this.transform.localToWorldMatrix, mat, 0);
    }
}
