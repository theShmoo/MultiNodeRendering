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
    private RawImage positionImage;

    private GameObject gBufferPanel;
    private Toggle toggleGBuffer;

    public GBuffer gBuffer;


    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {

        if (!gBuffer)
            gBuffer = Camera.main.GetComponent<GBuffer>();


        albedoImage = GameObject.Find("DiffuseImage").GetComponent<RawImage>();
        specularImage = GameObject.Find("SpecularImage").GetComponent<RawImage>();

        normalsImage = GameObject.Find("NormalsImage").GetComponent<RawImage>();
        positionImage = GameObject.Find("PositionImage").GetComponent<RawImage>();

        toggleGBuffer = GameObject.Find("ToggleGBuffer").GetComponent<Toggle>();
        gBufferPanel = GameObject.Find("GBufferPanel");        

    }




    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        UpdateGBufferImages();
    }




    /// <summary>
    /// Updates all UI components visualizing the gBuffer textures
    /// </summary>
    private void UpdateGBufferImages()
    {
        if (gBufferPanel.activeInHierarchy && gBuffer)
        {
            albedoImage.texture = gBuffer.AlbedoBufferTexture;
            specularImage.texture = gBuffer.SpecularBufferTexture;
            normalsImage.texture = gBuffer.NormalBufferTexture;
            positionImage.texture = gBuffer.PositionBufferTexture;
        }
    }



    /// <summary>
    /// 
    /// </summary>
    public void ShowGBuffer(bool enabled)
    {     
        gBufferPanel.SetActive(enabled);
    }
}
