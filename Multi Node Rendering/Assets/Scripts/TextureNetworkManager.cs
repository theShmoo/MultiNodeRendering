using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Camera))]
public class TextureNetworkManager : MonoBehaviour
{
    /// <summary>
    // Singleton instance
    /// </summary>
    private static TextureNetworkManager instance;
    /// <summary>
    /// Returns the instance of this NetworkManager
    /// </summary>
    public static TextureNetworkManager Instance
    {
        get { return instance; }
    }

    /// global settings:
    //@{
    public string m_ip = "127.0.0.1";
    public int m_port = 7075;
    [Range(5, 50)]
    public int m_maxNumberClients;
    private ConnectionConfig m_Config = null;
    private byte m_CommunicationChannel = 0;
    //@}

    /// Server Gui
    /// @{
    private float _OpacityValue = 1.0F;
    private bool _IsoSurfaceToggle = false;
    /// @}


    // client settings
    //@{
    /// <summary>
    /// This is the id of the connection from the client to the server
    /// The server also saves all this connection ids
    /// </summary>
    private int m_ClientConnectionId = 0;
    //@}

    // server settings
    //@{
    /// <summary>
    /// this is the id of the host this is the same for client and server
    /// </summary>
    private int m_HostId = 0;

    private HashSet<int> m_RendererClientIds = new HashSet<int>();

    public TileComposer m_tileComposer = null;
    public TileRaycaster m_tileRaycaster = null;
    //@}

    // internal parameters
    //@{
    /// <summary>
    /// If this network manager is started on the client or the server
    /// </summary>
    private bool _isStarted = false;
    /// <summary>
    /// If this is a server
    /// </summary>
    private bool _isServer = false;
    //@}

    public bool IsServer
    {
        get { return _isServer; }
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

	// Use this for initialization
	void Start () {
        //create configuration containing one reliable channel
        m_Config = new ConnectionConfig();
        m_CommunicationChannel = m_Config.AddChannel(QosType.UnreliableFragmented);

        // default number of clients is 12
        m_maxNumberClients = 12;
	}

    void OnGUI()
    {
        if (!_isStarted)
        {
            m_ip = GUI.TextField(new Rect(10, 10, 250, 30), m_ip, 25);
            m_port = Convert.ToInt32(GUI.TextField(new Rect(10, 40, 250, 30), m_port.ToString(), 25));
            if (GUI.Button(new Rect(10, 70, 250, 30), "start server"))
            {
                InitializeHost();
            }
            if (GUI.Button(new Rect(10, 100, 250, 30), "start client"))
            {
                InitializeClient();
            }
        }
        else
        {
            if (GUI.Button(new Rect(10, 10, 150, 30), "stop connection"))
            {
                StopConnection();
            }
            if (_isServer)
            {
                GUI.Label(new Rect(10, 50, 100, 30), "Opacity " + _OpacityValue.ToString("0.00") + ":");
                _OpacityValue = GUI.HorizontalSlider(new Rect(110, 50, 90, 30), _OpacityValue, 0.0F, 2.0F);
                this.m_tileComposer.Opacity = _OpacityValue;

                _IsoSurfaceToggle = GUI.Toggle(new Rect(10, 80, 150, 30), _IsoSurfaceToggle, "Iso Surface is " + (_IsoSurfaceToggle ? "On" : "Off"));
                this.m_tileComposer.Pass = _IsoSurfaceToggle ? 1 : 0;
            }
        }
    }

	// Update is called once per frame
	void Update () 
    {
	    if (!_isStarted)
            return;

        StartCoroutine("ReceiveNetworkEvents");
	}

    public void OnRaycasterParameterChanged(int pass, float opacity)
    {
        if(!_isServer)
            return;
        if ((pass < 0 || pass > 1) || (opacity < 0.0F || opacity > 2.0F))
        {
            Debug.LogError(String.Format("Invalid ray caster Parameters. pass {0} and opacity {1}",pass,opacity));
        }
        else
        {
            var msg = new RenderParameterMessage();
            msg.opacity = opacity;
            msg.pass = pass;

            SendMessageToAllClients(msg,RenderParameterMessage.MSG_ID);
        }
    }

    public void OnCameraParameterChanged()
    {
        if (!_isServer)
            return;

        // Set current state to TileRaycaster
        var msg = new RayCastStateMessage();
        msg.volumeWorldMatrix = Matrix4x4.identity;
        msg.viewMatrix = Camera.main.worldToCameraMatrix;
        msg.projectionMatrix = Camera.main.projectionMatrix;

        SendMessageToAllClients(msg, RayCastStateMessage.MSG_ID);
    }

    public void SendTextureToServer(Vector2 tileIndex, ref byte[] textureData)
    {
        if (_isServer)
            return;

        NetworkWriter writer = new NetworkWriter();
        writer.Write(tileIndex);
        writer.WriteBytesAndSize(textureData,textureData.Length);
        byte[] data = writer.ToArray();
        SendDataToServer(ref data);
    }

    private void OnClientsChanged()
    {
        int numClients = m_RendererClientIds.Count;
        if (numClients > 0)
        {
            // arrange the clients to a grid
            int iClientsX = System.Convert.ToInt32(Math.Log(numClients) / Math.Log(2.0));
            iClientsX = Math.Max(iClientsX, 1);
            int iClientsY = numClients / iClientsX;
            Vector2 vTiles = new Vector2(iClientsX, iClientsY);
            m_tileComposer.NumTilesChanged(vTiles);

            // send the clients where their tile is
            ArrangeTilesToClients();

            m_tileComposer.Active = true;
        }
        else
        {
            m_tileComposer.Active = false;
        }
    }

    private void SendMessageToAllClients(MessageBase msg, short msgType)
    {
        byte[] data = new byte[256];
        NetworkWriter netWriter = new NetworkWriter(data);
        netWriter.Write(msgType);
        msg.Serialize(netWriter);

        foreach (int client in m_RendererClientIds)
        {
            SendDataToClient(client, ref data);
        }
    }

    private void SendMessageToClient(MessageBase msg, short msgType, int clientConnectionId)
    {
        byte[] data = new byte[256];
        NetworkWriter netWriter = new NetworkWriter(data);
        netWriter.Write(msgType);
        msg.Serialize(netWriter);

        SendDataToClient(clientConnectionId, ref data);
    }

    IEnumerator ReceiveNetworkEvents()
    {
        int recHostId;
        int connectionId;
        int channelId;
        int bufferSize = 256;
        // bigger buffer size for the server!
        if(_isServer)
            bufferSize = 32768;
        byte[] recBuffer = new byte[bufferSize];
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        LogNetworkError(error);
        switch (recData)
        {
            case NetworkEventType.Nothing:
                break;

            case NetworkEventType.ConnectEvent:
                {
                    if (!_isServer)
                        OnServerConnect(recHostId, connectionId);
                    else
                        OnClientConnect(recHostId, connectionId);
                    Debug.Log(String.Format("Connect from host {0} connection {1}", recHostId, connectionId));
                    break;
                }

            case NetworkEventType.DataEvent:
                {
                    if (!_isServer)
                        OnDataFromServerReceived(dataSize, ref recBuffer);
                    else if (m_RendererClientIds.Contains(connectionId))
                        OnDataFromClientReceived(connectionId, dataSize, ref recBuffer);
                    else
                        Debug.Log("Error: unknown connection!");

                    break;
                }
            case NetworkEventType.DisconnectEvent:
                {
                    if (!_isServer)
                        OnServerDisconnect(recHostId, connectionId);
                    else
                        OnClientDisconnect(recHostId, connectionId);
                    break;
                }
        }

        yield return null;
    }
    /// <summary>
    /// Called on the client that the server disconnected
    /// </summary>
    /// <param name="recHostId">the id of the host this client connected to</param>
    /// <param name="connectionId">the id of the connection</param>
    private void OnServerDisconnect(int recHostId, int connectionId)
    {
        if (recHostId != m_HostId || connectionId != m_ClientConnectionId)
            Debug.LogError(String.Format("Error: Client Connection {0} to Host {1} is invalid!", recHostId, connectionId));

        Debug.Log(String.Format("DisConnect from host {0} connection {1}", recHostId, connectionId));
    }

    /// <summary>
    /// Called on the server that a client disconnected
    /// </summary>
    /// <param name="recHostId">the id of this host</param>
    /// <param name="connectionId">the id of the connection</param>
    private void OnClientDisconnect(int recHostId, int connectionId)
    {
        if (recHostId != m_HostId || m_RendererClientIds.Contains(connectionId) == false)
            Debug.LogError(String.Format("Error: Client Connection {0} to Host {1} is invalid!", recHostId, connectionId));

        Debug.Log(String.Format("DisConnect from host {0} connection {1}", recHostId, connectionId));

        m_RendererClientIds.Remove(connectionId);
        OnClientsChanged();
    }

    /// <summary>
    /// Called by a client when a new data package was received by the server
    /// </summary>
    /// <param name="dataSize"></param>
    /// <param name="recBuffer"></param>
    private void OnDataFromServerReceived(int dataSize, ref byte[] recBuffer)
    {
        NetworkReader netReader = new NetworkReader(recBuffer);
        short msgType = netReader.ReadInt16();
        if (msgType == RayCastStateMessage.MSG_ID)
        {
            var msg = netReader.ReadMessage<RayCastStateMessage>();
            m_tileRaycaster.RpcSetSceneState(msg);
        }
        else if (msgType == RenderParameterMessage.MSG_ID)
        {
            var msg = netReader.ReadMessage<RenderParameterMessage>();
            m_tileRaycaster.Pass = msg.pass;
            m_tileRaycaster.Opacity = msg.opacity;
        }
        else if (msgType == TileMessage.MSG_ID)
        {
            var msg = netReader.ReadMessage<TileMessage>();
            var tile = new ScreenTile();
            tile.setFromTileMessage(msg);
            m_tileRaycaster.RpcSetTile(tile);
        }
    }

    /// <summary>
    /// Called by the server when a client sent a new data package
    /// </summary>
    /// <param name="clientConnectionId"></param>
    /// <param name="dataSize"></param>
    /// <param name="recBuffer"></param>
    private void OnDataFromClientReceived(int clientConnectionId, int dataSize, ref byte[] recBuffer)
    {
        // receive the texture of a tile:
        NetworkReader netReader = new NetworkReader(recBuffer);
        Vector2 tileIndex = netReader.ReadVector2();
        byte[] data = netReader.ReadBytesAndSize();
        if (data != null)
        {
            this.m_tileComposer.SetTexture(tileIndex, data);
        }        
    }

    /// <summary>
    /// Send a data package to the specified client connection
    /// </summary>
    /// <param name="clientConnectionId"></param>
    /// <param name="bytes"></param>
    private void SendDataToClient(int clientConnectionId, ref byte[] bytes)
    {
        byte error;
        NetworkTransport.Send(m_HostId, clientConnectionId, m_CommunicationChannel, bytes, bytes.Length, out error);
        LogNetworkError(error);
    }

    /// <summary>
    /// Send a data package to the host
    /// </summary>
    /// <param name="bytes"></param>
    private void SendDataToServer(ref byte[] bytes)
    {
        byte error;
        NetworkTransport.Send(m_HostId, m_ClientConnectionId, m_CommunicationChannel, bytes, bytes.Length, out error);
        LogNetworkError(error);
    }

    /// <summary>
    /// Called by the client when connected to a server
    /// </summary>
    /// <param name="recHostId">the id of the host this client connected to</param>
    /// <param name="connectionId">the id of the connection</param>
    private void OnServerConnect(int recHostId, int connectionId)
    {
        if (recHostId != m_HostId || connectionId != m_ClientConnectionId)
            Debug.LogError(String.Format("Error: Client Connection {0} to Host {1} is invalid!", recHostId, connectionId));
    }

    /// <summary>
    /// Called on the server that a new client connected
    /// </summary>
    /// <param name="recHostId">the id of the host this client connected to</param>
    /// <param name="connectionId">the id of the connection</param>
    private void OnClientConnect(int recHostId, int connectionId)
    {
        if (recHostId != m_HostId)
            Debug.LogError(String.Format("Error: Host {0} is invalid!", recHostId));

        m_RendererClientIds.Add(connectionId);

        OnClientsChanged();
    }

    /// <summary>
    /// Log any network errors to the console.
    /// </summary>
    /// <param name="error">Error.</param>
    private void LogNetworkError(byte error)
    {
        if (error != (byte)NetworkError.Ok)
        {
            NetworkError nerror = (NetworkError)error;
            Debug.Log("Error: " + nerror.ToString());
        }
    }

    /// <summary>
    /// Start the host
    /// </summary>
    private void InitializeHost()
    {
        _isStarted = true;
        _isServer = true;

        NetworkTransport.Init();
        HostTopology topology = new HostTopology(m_Config, m_maxNumberClients);
        m_HostId = NetworkTransport.AddHost(topology, m_port, null);

        m_tileComposer = Camera.main.GetComponent<TileComposer>();
        m_tileRaycaster = Camera.main.GetComponent<TileRaycaster>();
        m_tileComposer.enabled = true;
        m_tileRaycaster.enabled = false;
    }

    /// <summary>
    /// Initialize a new client
    /// </summary>
    private void InitializeClient()
    {
        _isStarted = true;
        _isServer = false;

        NetworkTransport.Init();
        HostTopology topology = new HostTopology(m_Config, m_maxNumberClients);

        //any port for udp client, for websocket second parameter is ignored, as webgl based game can be client only
        m_HostId = NetworkTransport.AddHost(topology, 0);
        byte error;
        m_ClientConnectionId = NetworkTransport.Connect(m_HostId, m_ip, m_port, 0, out error);

        m_tileComposer = Camera.main.GetComponent<TileComposer>();
        m_tileRaycaster = Camera.main.GetComponent<TileRaycaster>();
        m_tileComposer.enabled = false;
        m_tileRaycaster.enabled = true;
    }

    /// <summary>
    /// Shutdown the network
    /// </summary>
    private void StopConnection()
    {
        _isStarted = false;
        NetworkTransport.Shutdown();
    }

    private void ArrangeTilesToClients()
    {
        var tiles = m_tileComposer.tiles;
        int i = 0; 
        foreach (int client in m_RendererClientIds)
        {
            var msg = tiles[i].GetTileMessage();
            SendMessageToClient(msg,TileMessage.MSG_ID,client);
            i++;
            if (i >= tiles.Count)
                break;
        }
    }

}
