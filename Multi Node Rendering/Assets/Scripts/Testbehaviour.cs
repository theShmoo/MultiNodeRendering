using UnityEngine;
using System.Collections;

public class Testbehaviour : MonoBehaviour
{
   


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

            this.gameObject.transform.Rotate(Vector3.up, 90.0f * Time.deltaTime);
        
    }
}
