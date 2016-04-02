using UnityEngine;

using System.Collections;


    [ExecuteInEditMode]
public class TestBehaviour : MonoBehaviour
{


    public Mesh mesh;
    public Material mat;

    
 


    void OnRenderObject()
    {
        mat.SetPass(0);
        Graphics.DrawMeshNow(mesh, this.transform.localToWorldMatrix);
    }


    // Update is called once per frame
    void Update()
    {     
        this.gameObject.transform.Rotate(Vector3.up, 90.0f * Time.deltaTime);
    }
}
