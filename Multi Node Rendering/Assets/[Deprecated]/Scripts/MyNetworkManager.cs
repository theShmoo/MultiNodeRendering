using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class MyMsgTypes
{
    public static short MSG_CAMERA_TRANSFORM = 1000;
};

public class CameraTransformMessage : MessageBase
{
    public Transform cameraTransform;
};

/// <summary>
/// This Script handles the network management between the server and the render nodes (client)
/// </summary>
public class MyNetworkManager : MonoBehaviour
{

    public enum ConnectionState
    {
        NOT_CONNECTED = 0, RUNNING_AS_SERVER = 1, RUNNING_AS_RENDER_NODE = 2, RUNNING_OFFLINE = 3
    };

    /// <summary>
    // Singleton instance
    /// </summary>
    private static MyNetworkManager instance;
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
    private List<HostData> hostList;
    /// <summary>
    /// The selected host that this render node will connect to or is connect to
    /// </summary>
    private HostData selectedHost = null;

    NetworkClient myClient;

    /// <summary>
    /// A list of all connected nodes.
    /// </summary>
    private List<NetworkPlayer> connectedNodes; 

    /// <summary>
    // The name of the server to register on the master server
    /// </summary>
    public string typeName= "MultiNodeRendering";
    /// <summary>
    // The name of the instance of the server on the master server
    /// </summary>
    public string gameName = "Server";
    /// <summary>
    ///  maximum nummer of connections
    /// </summary>
    private int iMaxNumberOfConnections = 4;
    /// <summary>
    /// The port of the server
    /// </summary>
    public int port = 4000;

    /// <summary>
    /// Returns the instance of this NetworkManager
    /// </summary>
    public static MyNetworkManager Instance
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

    public List<string> GetHostNames()
    {
        if(hostList == null)
        {
            return null;
        }
        List<string> hostNames = new List<string>(hostList.Count);
        foreach (HostData host in hostList)
        {
            hostNames.Add(host.gameType);
        }
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

        hostList = new List<HostData>();

        connectedNodes = new List<NetworkPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (State == ConnectionState.RUNNING_AS_SERVER)
        {
            var msg = new CameraTransformMessage();
            msg.cameraTransform = Camera.main.gameObject.transform;

            NetworkServer.SendToAll(MyMsgTypes.MSG_CAMERA_TRANSFORM, msg);
        }
    }

    /// <summary>
    /// Disconnects from the server if it is a render node
    /// Disconnects all render nodes if it is a server
    /// </summary>
    private void Disconnect(ConnectionState _state)
    {
        if (_state == ConnectionState.RUNNING_AS_SERVER)
        {
            Network.Disconnect();
            MasterServer.UnregisterHost();
        }
        else if (_state == ConnectionState.RUNNING_AS_RENDER_NODE)
        {
            Network.Disconnect();
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
        switch (State)
        {
            case ConnectionState.NOT_CONNECTED:
                if (previousState == ConnectionState.RUNNING_AS_RENDER_NODE || previousState == ConnectionState.RUNNING_AS_SERVER)
                {
                    Disconnect(previousState);
                }
                Camera.main.GetComponent<TileComposer>().Active = false;
                break;

            case ConnectionState.RUNNING_OFFLINE:
                if (previousState == ConnectionState.RUNNING_AS_RENDER_NODE || previousState == ConnectionState.RUNNING_AS_SERVER)
                {
                    Disconnect(previousState);
                }
                Camera.main.GetComponent<TileComposer>().Active = true;
                break;

            case ConnectionState.RUNNING_AS_RENDER_NODE:
                if (previousState == ConnectionState.NOT_CONNECTED || previousState == ConnectionState.RUNNING_OFFLINE)
                {
                   JoinServer();
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
        Network.InitializeServer(iMaxNumberOfConnections, port, !Network.HavePublicAddress());
        MasterServer.RegisterHost(typeName, gameName);
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
        {
            myClient = new NetworkClient();
            myClient.RegisterHandler(MyMsgTypes.MSG_CAMERA_TRANSFORM, OnCameraTransformMessage);
            myClient.Connect(selectedHost.ip[0], selectedHost.port);
        }
        else
            Debug.Log("No Host Selected");
    }

    /// <summary>
    /// When a client joins a server the client gets this notification
    /// </summary>
    void OnConnectedToServer()
    {
        Debug.Log("Server Joined ");
    }

    /// <summary>
    /// When a client disconnects from a server the client gets this notification
    /// </summary>
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("Disconnected from server: " + info);
        if (State == ConnectionState.RUNNING_AS_RENDER_NODE)
        {
            State = ConnectionState.NOT_CONNECTED;
        }
    }

    /// <summary>
    /// When a client joins a server the server gets this notification
    /// </summary>
    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player connected from " + player.ipAddress + ":" + player.port);
        connectedNodes.Add(player);
    }

    /// <summary>
    /// When a client disconnects from a server the server gets this notification
    /// </summary>
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Clean up after player " + player);
        connectedNodes.Remove(player);
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

    /// <summary>
    /// Refresh the list of hosts on the master server
    /// </summary>
    public void RefreshHostList()
    {
        if (!isRefreshingHostList)
        {
            isRefreshingHostList = true;
            MasterServer.RequestHostList(typeName);
        }
    }

    /// <summary>
    /// Called when the master server gets an event
    /// </summary>
    void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.HostListReceived)
        {
            HostData[] hosts = MasterServer.PollHostList();
            foreach (HostData host in hosts)
            {
                hostList.Add(host);
            }
        }
        else if(msEvent != MasterServerEvent.RegistrationSucceeded)
        {
            State = ConnectionState.NOT_CONNECTED;
        }
    }

    void OnCameraTransformMessage(NetworkMessage netMsg)
    {
        Debug.Log(netMsg);
    }

    public void SetHost(int iHostId)
    {
        if (iHostId < 0 || iHostId > hostList.Count)
            Debug.LogError("Invalid Host ID");

        selectedHost = hostList[iHostId];
    }

    public void SetNumberOfTiles(int iNumTilesWidth,int iNumTilesHeigth)
    {
        if (iNumTilesWidth < 0 || iNumTilesHeigth < 0)
            Debug.LogError("Invalid Number of Tiles");
        if (State != ConnectionState.RUNNING_AS_SERVER)
            Debug.LogError("Must run as server to change the number of tiles");
        if (iNumTilesWidth * iNumTilesHeigth > iMaxNumberOfConnections)
            Debug.LogError("This exceeds the maximum number of connections");
        Vector2 numTiles = new Vector2(iNumTilesWidth, iNumTilesHeigth);

        TileComposer tileComposer = Camera.main.GetComponent<TileComposer>();
        tileComposer.NumTilesChanged(numTiles);
    }
}
