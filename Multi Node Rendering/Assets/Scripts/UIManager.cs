using UnityEngine;
using System.Collections;
using UnityEngine.UI;



/// <summary>
/// The UIManager handles communication between the user interface and the several data models. 
/// </summary>
public class UIManager : MonoBehaviour
{

    // Images to display the content of the gBuffer
    private RawImage albedoImage;
    private RawImage specularImage;
    private RawImage normalsImage;
    private RawImage emmisionImage;
    private RawImage depthImage;

    //public GBufferInterface gBuffer;


    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {

        if (!gBuffer)
           // gBuffer = Camera.main.GetComponent<GBufferInterface>();


        albedoImage = GameObject.Find("AlbedoImage").GetComponent<RawImage>();
        specularImage = GameObject.Find("SpecularImage").GetComponent<RawImage>();

        normalsImage = GameObject.Find("NormalsImage").GetComponent<RawImage>();
        emmisionImage = GameObject.Find("EmissionImage").GetComponent<RawImage>();
        depthImage = GameObject.Find("DepthImage").GetComponent<RawImage>();

    }




    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        Graphics.SetRenderTarget(null);
        UpdateGBufferImages();
    }




    /// <summary>
    /// Updates all UI components visualizing the gBuffer textures
    /// </summary>
    private void UpdateGBufferImages()
    {
        //albedoImage.texture = gBuffer.Albedo;
        //specularImage.texture = gBuffer.Specular;
        //normalsImage.texture = gBuffer.Normals;
        //emmisionImage.texture = gBuffer.Emission;
        //depthImage.texture = gBuffer.Depth;
    }
}
