using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class TileNetworkManager : NetworkManager {

    /// <summary>
    // Singleton instance
    /// </summary>
    private static TileNetworkManager instance;
    
    // listener on textures
    public int nNumClients = 0;
    public TileComposer tileComposer = null;
    public NetworkConnection hostConnection = null;
    //public TextureTCPReceiver textureReceiver = null;

    /// <summary>
    /// Returns the instance of this NetworkManager
    /// </summary>
    public static TileNetworkManager Instance
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

    // Use this for initialization
    void Start()
    {
        ServerInit();
    }

    void ServerInit()
    {
//        NetworkServer.SetNetworkConnectionClass<DebugConnection>();
        nNumClients = 0;
//         if (textureReceiver == null)
//             textureReceiver = this.GetComponent<TextureTCPReceiver>();
    }

    // called when a client disconnects
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log("Client disconnected");

        foreach (PlayerController player in conn.playerControllers)
            OnServerRemovePlayer(conn, player);

        NetworkServer.DestroyPlayersForConnection(conn);
    }

    // called when a client is ready
    public override void OnServerReady(NetworkConnection conn)
    {
        Debug.Log("Client ready");
        NetworkServer.SetClientReady(conn);
    }

    // called when a new player is added for a client
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("a player " + playerControllerId + " is added for a client");
        var player = (GameObject)GameObject.Instantiate(playerPrefab, new Vector3(0,0,0) , Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

        // the first client that connects becomes the composer.
        if (tileComposer == null)
        {
            if (nNumClients > 0)
                Debug.LogError("The first client that connects should be the composer!");
            tileComposer = player.GetComponent<TileComposer>();
            hostConnection = conn;
        }
        else
        {
            nNumClients++;
        }
        ItlSetTileComposerByClients();
    }

    // called when a player is removed for a client
    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        Debug.Log("a player " + player.playerControllerId + " is removed for a client");
        nNumClients--;
        ItlSetTileComposerByClients();
        if (player.gameObject != null)
            NetworkServer.Destroy(player.gameObject);
    }

    private void ItlSetTileComposerByClients()
    {
        if (nNumClients > 0)
        {
            int iClientsX = System.Convert.ToInt32(Math.Log(nNumClients) / Math.Log(2.0));
            iClientsX = Math.Max(iClientsX, 1);
            int iClientsY = nNumClients / iClientsX;
            Vector2 vTiles = new Vector2(iClientsX, iClientsY);
            tileComposer.NumTilesChanged(vTiles);
            //tileComposer.ArrangeTilesToRaycaster(hostConnection.connectionId);
            tileComposer.Active = true;
        }
        else
        {
            tileComposer.Active = false;
        }
    }
}
