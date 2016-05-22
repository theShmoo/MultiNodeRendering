using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
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
    private Dropdown hostListDropdown;
    [SerializeField]
    private InputField numberTilesWidth;
    [SerializeField]
    private InputField numberTilesHeight;
    [SerializeField]
    private Button startRenderNodeButton;
    [SerializeField]
    private Button changeNumberOfTilesButton;
    [SerializeField]
    private Toggle toggleNetworkInfo;

    //--------------------------------------------------------------------------------------
    // UI Elements for Network Status Panel
    //--------------------------------------------------------------------------------------
    [SerializeField]
    private GameObject networkStatusPanel;

    [SerializeField]
    private GameObject networkServerPanel;

    //--------------------------------------------------------------------------------------
    // Dataset to be displayed
    //--------------------------------------------------------------------------------------
    public GBuffer gBuffer;

    //--------------------------------------------------------------------------------------
    // Singleton instance
    //--------------------------------------------------------------------------------------
    private static UIManager instance;

    /// <summary>
    /// Returns the instance of this UIManager
    /// </summary>
    public static UIManager Instance
    {
        get { return instance; }
    }

    /// <summary>
    /// This Function is called once the script awakes, even before Enable() and Start()
    /// </summary>
    private void Awake()
    {
        // Destroy other Instances of this script
        if (instance && instance != this)
        {
            Destroy(this.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
//        if (!gBuffer) gBuffer = Camera.main.GetComponent<GBuffer>();

        // GBuffer UI
//         if (!toggleGBuffer) toggleGBuffer = GameObject.Find("ToggleGBufferInfo").GetComponent<Toggle>();
//         if (!gBufferPanel) gBufferPanel = GameObject.Find("GBufferPanel");
//         if (!albedoImage) albedoImage = GameObject.Find("DiffuseImage").GetComponent<RawImage>();
//         if (!specularImage) specularImage = GameObject.Find("SpecularImage").GetComponent<RawImage>();
//         if (!normalsImage) normalsImage = GameObject.Find("NormalsImage").GetComponent<RawImage>();
//         if (!positionImage) positionImage = GameObject.Find("PositionImage").GetComponent<RawImage>();


        // Network Connect UI
        if (!toggleNetworkInfo) toggleNetworkInfo = GameObject.Find("ToggleNetworkInfo").GetComponent<Toggle>();
        if (!networkConnectPanel) networkConnectPanel = GameObject.Find("NetworkConnectPanel");
        if (!hostListDropdown) hostListDropdown = GameObject.Find("HostListDropdown").GetComponent<Dropdown>();
        if (!numberTilesWidth) numberTilesWidth = GameObject.Find("TilesWidthInput").GetComponent<InputField>();
        numberTilesWidth.contentType = InputField.ContentType.IntegerNumber;
        if (!numberTilesHeight) numberTilesHeight = GameObject.Find("TilesHeightInput").GetComponent<InputField>();
        numberTilesHeight.contentType = InputField.ContentType.IntegerNumber;
        if (!startRenderNodeButton) startRenderNodeButton = GameObject.Find("StartRenderNodeButton").GetComponent<Button>();
        if (!changeNumberOfTilesButton) changeNumberOfTilesButton = GameObject.Find("ChangeNumberTilesButton").GetComponent<Button>();

        // Network Status UI
        if (!networkStatusPanel) networkStatusPanel = GameObject.Find("NetworkStatusPanel");
        if (!networkServerPanel) networkServerPanel = GameObject.Find("NetworkServerPanel");

        UpdateNetworkUI();
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // Updates visualization of gBuffer textures
        if (gBufferPanel.activeInHierarchy && gBuffer != null)
        {
            albedoImage.texture = gBuffer.AlbedoBufferTexture;
            specularImage.texture = gBuffer.SpecularBufferTexture;
            normalsImage.texture = gBuffer.NormalBufferTexture;
            positionImage.texture = gBuffer.PositionBufferTexture;
        }
        //UpdateNetworkUI();
    }

    /// <summary>
    /// Updates the UI depending on the network state
    /// </summary>
    public void UpdateNetworkUI()
    {
        switch (MyNetworkManager.Instance.State)
        {
            case MyNetworkManager.ConnectionState.NOT_CONNECTED:
                networkConnectPanel.SetActive(toggleNetworkInfo.isOn);
                networkStatusPanel.SetActive(false);
                networkServerPanel.SetActive(false);

                //toggleGBuffer.gameObject.SetActive(false);
                //gBufferPanel.SetActive(false);

                hostListDropdown.ClearOptions();
                List<string> hostNames = MyNetworkManager.Instance.GetHostNames();
                if (hostNames == null || hostNames.Count == 0)
                {
                    hostListDropdown.options.Add(new Dropdown.OptionData("No Hosts available"));
                    startRenderNodeButton.enabled = false;
                    hostListDropdown.value = 0;
                }
                else
                {
                    foreach (string hostName in hostNames)
                    {
                        hostListDropdown.options.Add(new Dropdown.OptionData(hostName));
                    }
                    startRenderNodeButton.enabled = true;
                }
                hostListDropdown.RefreshShownValue();
                break;

            case MyNetworkManager.ConnectionState.RUNNING_OFFLINE:

                networkConnectPanel.SetActive(toggleNetworkInfo.isOn);
                networkStatusPanel.SetActive(false);

                //toggleGBuffer.gameObject.SetActive(true);
                //gBufferPanel.SetActive(toggleGBuffer.isOn);

                break;

            case MyNetworkManager.ConnectionState.RUNNING_AS_RENDER_NODE:
                networkConnectPanel.SetActive(false);
                networkStatusPanel.SetActive(toggleNetworkInfo.isOn);

                //toggleGBuffer.gameObject.SetActive(true);
                //gBufferPanel.SetActive(toggleGBuffer.isOn);

                break;

            case MyNetworkManager.ConnectionState.RUNNING_AS_SERVER:
                networkConnectPanel.SetActive(false);
                networkStatusPanel.SetActive(toggleNetworkInfo.isOn);
                networkServerPanel.SetActive(toggleNetworkInfo.isOn);

                //toggleGBuffer.gameObject.SetActive(true);
                //gBufferPanel.SetActive(toggleGBuffer.isOn);

                break;
        }
    }

    /// <summary>
    /// Starts a new Server (Composer) 
    /// </summary>
    public void StartServerButtonClicked()
    {
        MyNetworkManager.Instance.State = MyNetworkManager.ConnectionState.RUNNING_AS_SERVER;
    }

    /// <summary>
    /// Connects a Render Node to the given Server using the ip/port information
    /// </summary>
    public void StartClientButtonClicked()
    {
        MyNetworkManager.Instance.SetHost(hostListDropdown.value);
        MyNetworkManager.Instance.State = MyNetworkManager.ConnectionState.RUNNING_AS_RENDER_NODE;
    }

    /// <summary>
    /// Starts the Rendering in Offline Mode
    /// </summary>
    public void OfflineModeButtonClicked()
    {
        MyNetworkManager.Instance.State = MyNetworkManager.ConnectionState.RUNNING_OFFLINE;
    }

    /// <summary>
    /// Starts the Rendering in Offline Mode
    /// </summary>
    public void RefreshHostsButtonClicked()
    {
        MyNetworkManager.Instance.RefreshHostList();
        UpdateNetworkUI();
    }

    /// <summary>
    /// Disconnects the current application from the network
    /// </summary>
    public void DisconnectButtonClicked()
    {
        MyNetworkManager.Instance.State = MyNetworkManager.ConnectionState.NOT_CONNECTED;
    }

    /// <summary>
    /// Changes the number of tiles of the current server
    /// </summary>
    public void ChangeNumberTilesButtonClicked()
    {
        int iNumTilesWidth = 0;
        int iNumTilesHeight = 0;
        bool parsed = Int32.TryParse(numberTilesWidth.text, out iNumTilesWidth);
        if (parsed)
            Int32.TryParse(numberTilesHeight.text, out iNumTilesHeight);
        if (parsed && iNumTilesWidth > 0 && iNumTilesHeight > 0)
            MyNetworkManager.Instance.SetNumberOfTiles(iNumTilesWidth,iNumTilesHeight);
    }
}
