using UnityEngine;
using System.Collections;

/// <summary>
/// This Script handles the network management between the server and the render nodes (client)
/// </summary>
public class NetworkManager : MonoBehaviour
{

    public enum ConnectionState
    {
        NOT_CONNECTED = 0, RUNNING_AS_SERVER = 1, RUNNING_AS_RENDER_NODE = 2, RUNNING_OFFLINE = 3
    };

    /// <summary>
    // Singleton instance
    /// </summary>
    private static NetworkManager instance;
    /// <summary>
    // Current connection state
    /// </summary>
    private ConnectionState state;
    /// <summary>
    // Previous connection state
    /// </summary>
    private ConnectionState previousState;
    /// <summary>
    ///  Whether the host list is refreshing
    /// </summary>
    private bool isRefreshingHostList = false;
    /// <summary>
    ///  A list of hosts to connect to
    /// </summary>
    private HostData[] hostList;
    /// <summary>
    /// The selected host that this render node will connect to or is connect to
    /// </summary>
    private HostData selectedHost = null;

    /// <summary>
    // The name of the server to register on the master server
    /// </summary>
    public const string TYPE_NAME= "MultiNodeRendering";
    /// <summary>
    // The name of the instance of the server on the master server
    /// </summary>
    public const string GAME_NAME = "Server";
    /// <summary>
    ///  maximum nummer of connections
    /// </summary>
    private const int MAX_NUMBER_CONNECTIONS = 1;
    /// <summary>
    /// The port of the server
    /// </summary>
    public const int PORT = 4000;

    /// <summary>
    /// Returns the instance of this NetworkManager
    /// </summary>
    public static NetworkManager Instance
    {
        get { return instance; }
    }

    /// <summary>
    /// Returns the state of the current connection
    /// </summary>
    public ConnectionState State
    {
        get { return state; }
        set {
            previousState = state;
            state = value;
            OnStateChanged();
        }
    }

    public string[] GetHostNames()
    {
        if(hostList == null)
        {
            return null;
        }
        string[] hostNames = new string[hostList.Length];
        for (int i = 0; i < hostList.Length; i++ )
            hostNames[i] = hostList[i].gameType;
        return hostNames;
    }

    /// <summary>
    /// This Function is called once the script awakes, even before Enable() and Start()
    /// </summary>
    private void Awake()
    {
        // Destroy other Instances of this script
        if(instance && instance != this)
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
        previousState = ConnectionState.NOT_CONNECTED;
        state = ConnectionState.NOT_CONNECTED;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Disconnects from the server if it is a render node
    /// Disconnects all render nodes if it is a server
    /// </summary>
    private void Disconnect(ConnectionState state)
    {
        if (state == ConnectionState.RUNNING_AS_SERVER)
        {
            Debug.Log("Server Disconnected!");
        }
        else if(state == ConnectionState.RUNNING_AS_RENDER_NODE)
        {
            Debug.Log("Render Node Disconnected!");
        }
        else
        {
            Debug.LogError("Error Disconnecting!");
        }
    }

    /// <summary>
    /// Is called when the state changes
    /// </summary>
    private void OnStateChanged()
    {
        switch (state)
        {
            case ConnectionState.NOT_CONNECTED:
                if (previousState == ConnectionState.RUNNING_AS_RENDER_NODE || previousState == ConnectionState.RUNNING_AS_SERVER)
                {
                    Disconnect(previousState);
                }
                Camera.main.GetComponent<DeferredRenderer>().Active = false;
                break;

            case ConnectionState.RUNNING_OFFLINE:
                if (previousState == ConnectionState.RUNNING_AS_RENDER_NODE || previousState == ConnectionState.RUNNING_AS_SERVER)
                {
                    Disconnect(previousState);
                }
                Camera.main.GetComponent<DeferredRenderer>().Active = true;
                break;

            case ConnectionState.RUNNING_AS_RENDER_NODE:
                if (previousState == ConnectionState.NOT_CONNECTED || previousState == ConnectionState.RUNNING_OFFLINE)
                {
                   // JoinServer();
                }
                else
                {
                    Debug.LogError("Error Starting Render Node!");
                }
                break;
            case ConnectionState.RUNNING_AS_SERVER:
                if (previousState == ConnectionState.NOT_CONNECTED || previousState == ConnectionState.RUNNING_OFFLINE)
                    StartServer();
                else
                    Debug.LogError("Error Starting Server!");
                break;
        }

        UIManager.Instance.UpdateNetworkUI();
    }

    /// <summary>
    /// Starts the host
    /// </summary>
    private void StartServer()
    {
        Network.InitializeServer(MAX_NUMBER_CONNECTIONS, PORT, !Network.HavePublicAddress());
        MasterServer.RegisterHost(TYPE_NAME, GAME_NAME);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnServerInitialized()
    {
        Debug.Log("Server Initialized");
    }

    /// <summary>
    /// 
    /// </summary>
    private void JoinServer()
    {
        if (selectedHost != null)
            Network.Connect(selectedHost);
        else
            Debug.Log("No Host Selected");
    }

    /// <summary>
    /// 
    /// </summary>
    void OnConnectedToServer()
    {
        Debug.Log("Server Joined");
    }

    /// <summary>
    /// Refresh the list of hosts on the master server
    /// </summary>
    public void RefreshHostList()
    {
        if (!isRefreshingHostList)
        {
            isRefreshingHostList = true;
            MasterServer.RequestHostList(NetworkManager.TYPE_NAME);
        }
    }

    /// <summary>
    /// Called when the master server gets an event
    /// </summary>
    void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.HostListReceived)
            hostList = MasterServer.PollHostList();
    }

    public void SetHost(int iHostId)
    {
        if (iHostId < 0 || iHostId > hostList.Length)
            Debug.LogError("Invalid Host ID");

        selectedHost = hostList[iHostId];
    }
}
