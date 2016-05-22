using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class TileNetworkManager : NetworkManager {

    public List<GameObject> clients = null;
    public TileComposer tileComposer = null;

    // Use this for initialization
    void Start()
    {
        ServerInit();
    }

    void ServerInit()
    {
        clients = new List<GameObject>();
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
            if (clients.Count != 0)
                Debug.LogError("The first client that connects should be the composer!");
            tileComposer = player.GetComponent<TileComposer>();
        }
        else
        {
            clients.Add(player.gameObject);
        }
        ItlSetTileComposerByClients();
    }

    // called when a player is removed for a client
    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        Debug.Log("a player " + player.playerControllerId + " is removed for a client");
        clients.Remove(player.gameObject);
        ItlSetTileComposerByClients();
        if (player.gameObject != null)
            NetworkServer.Destroy(player.gameObject);
    }

    private void ItlSetTileComposerByClients()
    {
        if (clients.Count > 0)
        {
            double dNumClients = System.Convert.ToDouble(clients.Count);
            int iClientsX = System.Convert.ToInt32(Math.Log(dNumClients) / Math.Log(2.0));
            iClientsX = Math.Max(iClientsX, 1);
            int iClientsY = clients.Count / iClientsX;
            //int iOverflow = clients.Count - (iClientsX * iClientsY);
            Vector2 vTiles = new Vector2(iClientsX, iClientsY);
            tileComposer.NumTilesChanged(vTiles);
            List<TileRaycaster> raycaster = new List<TileRaycaster>();
            foreach (var c in clients)
            {
                raycaster.Add(c.GetComponent<TileRaycaster>());
            }
            tileComposer.ArrangeTilesToRaycaster(raycaster);
            tileComposer.Active = true;
        }
        else
        {
            tileComposer.Active = false;
        }
    }
}
