using UnityEngine;
using System.Collections;
using UnityEngine.UI;



/// <summary>
/// The UIManager handles communication between the user interface and the several data models. 
/// </summary>

public class UIManager : MonoBehaviour
{



    //--------------------------------------------------------------------------------------
    // UI Elements for GBuffer Panel
    //--------------------------------------------------------------------------------------
    [SerializeField]
    private RawImage albedoImage;
    [SerializeField]
    private RawImage specularImage;
    [SerializeField]
    private RawImage normalsImage;
    [SerializeField]
    private RawImage positionImage;

    [SerializeField]
    private GameObject gBufferPanel;
    [SerializeField]
    private Toggle toggleGBuffer;



    //--------------------------------------------------------------------------------------
    // UI Elements for Network Connect Panel
    //--------------------------------------------------------------------------------------
    [SerializeField]
    private GameObject networkConnectPanel;
    [SerializeField]
    private Text ipTextField;
    [SerializeField]
    private Text portTextField;
    [SerializeField]
    private Toggle toggleNetworkInfo;




    //--------------------------------------------------------------------------------------
    // UI Elements for Network Status Panel
    //--------------------------------------------------------------------------------------
    [SerializeField]
    private GameObject networkStatusPanel;
    //--------------------------------------------------------------------------------------
    // Dataset to be displayed
    //--------------------------------------------------------------------------------------
    public GBuffer gBuffer;


    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
        if (!gBuffer) gBuffer = Camera.main.GetComponent<GBuffer>();

        // GBuffer UI
        if (!toggleGBuffer) toggleGBuffer = GameObject.Find("ToggleGBufferInfo").GetComponent<Toggle>();
        if (!gBufferPanel) gBufferPanel = GameObject.Find("GBufferPanel");
        if (!albedoImage) albedoImage = GameObject.Find("DiffuseImage").GetComponent<RawImage>();
        if (!specularImage) specularImage = GameObject.Find("SpecularImage").GetComponent<RawImage>();
        if (!normalsImage) normalsImage = GameObject.Find("NormalsImage").GetComponent<RawImage>();
        if (!positionImage) positionImage = GameObject.Find("PositionImage").GetComponent<RawImage>();


        // Network Connect UI
        if (!toggleNetworkInfo) toggleNetworkInfo = GameObject.Find("ToggleNetworkInfo").GetComponent<Toggle>();
        if (!networkConnectPanel) networkConnectPanel = GameObject.Find("NetworkConnectPanel");
        if (!ipTextField) ipTextField = GameObject.Find("IP").GetComponent<Text>();
        if (!portTextField) portTextField = GameObject.Find("Port").GetComponent<Text>();


        // Network Status UI
        if (!networkStatusPanel) networkStatusPanel = GameObject.Find("NetworkStatusPanel");

    }




    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {

        // Show correct UI depending on Network State
        switch (NetworkManager.Instance.State)
        {
            case NetworkManager.ConnectionState.NOT_CONNECTED:               
                networkConnectPanel.SetActive(toggleNetworkInfo.isOn);
                networkStatusPanel.SetActive(false);
                
                toggleGBuffer.gameObject.SetActive(false);
                gBufferPanel.SetActive(false);

                // Remove this!!! Renderer State not changed by UI, but by NetworkManager later on!!
                Camera.main.GetComponent<DeferredRenderer>().Active = false;

                break;
            
            case NetworkManager.ConnectionState.RUNNING_OFFLINE:               
                networkConnectPanel.SetActive(toggleNetworkInfo.isOn);
                networkStatusPanel.SetActive(false);
                
                toggleGBuffer.gameObject.SetActive(true);
                gBufferPanel.SetActive(toggleGBuffer.isOn);


                // Remove this!!! Renderer State not changed by UI, but by NetworkManager later on!!
                Camera.main.GetComponent<DeferredRenderer>().Active = true;


                break;
            
            case NetworkManager.ConnectionState.RUNNING_AS_RENDER_NODE:               
                networkConnectPanel.SetActive(false);
                networkStatusPanel.SetActive(toggleNetworkInfo.isOn);
                
                toggleGBuffer.gameObject.SetActive(true);
                gBufferPanel.SetActive(toggleGBuffer.isOn);

                // Remove this!!! Renderer State not changed by UI, but by NetworkManager later on!!
                Camera.main.GetComponent<DeferredRenderer>().Active = true;
                break;
            
            case NetworkManager.ConnectionState.RUNNING_AS_SERVER:
                networkConnectPanel.SetActive(false);
                networkStatusPanel.SetActive(toggleNetworkInfo.isOn);
                
                toggleGBuffer.gameObject.SetActive(true);
                gBufferPanel.SetActive(toggleGBuffer.isOn);

                // Remove this!!! Renderer State not changed by UI, but by NetworkManager later on!!
                Camera.main.GetComponent<DeferredRenderer>().Active = true;
                break;
        }

        // Updates viszualization of gBuffer textures

        if (gBufferPanel.activeInHierarchy && gBuffer!= null)
        {
            albedoImage.texture = gBuffer.AlbedoBufferTexture;
            specularImage.texture = gBuffer.SpecularBufferTexture;
            normalsImage.texture = gBuffer.NormalBufferTexture;
            positionImage.texture = gBuffer.PositionBufferTexture;
        }
    }




    /// <summary>
    /// Starts a new Server (Composer) 
    /// </summary>
    public void StartServerButtonClicked()
    {
        NetworkManager.Instance.State = NetworkManager.ConnectionState.RUNNING_AS_SERVER;
        // TODO: Start Server
    }




    /// <summary>
    /// Connects a Render Node to the given Server using the ip/port information
    /// </summary>
    public void StartClientButtonClicked()
    {
        NetworkManager.Instance.State = NetworkManager.ConnectionState.RUNNING_AS_RENDER_NODE;
        // TODO: Connect to Server
    }




    /// <summary>
    /// Starts the Rendering in Offline Mode
    /// </summary>
    public void OfflineModeButtonClicked()
    {
        NetworkManager.Instance.State = NetworkManager.ConnectionState.RUNNING_OFFLINE;
        // TODO: Start Offline Mode (Use the DeferredRenderer)
    }




    /// <summary>
    /// Disconnects the current application from the network
    /// </summary>
    public void DisconnectButtonClicked()
    {
        if (NetworkManager.Instance.State == NetworkManager.ConnectionState.RUNNING_AS_RENDER_NODE)
        {
            // TODO: Close connection to Server
        }
        if (NetworkManager.Instance.State == NetworkManager.ConnectionState.RUNNING_AS_SERVER)
        {
            // TODO: Close Server and connections to all clients
        }

        NetworkManager.Instance.State = NetworkManager.ConnectionState.NOT_CONNECTED;
    }






}
