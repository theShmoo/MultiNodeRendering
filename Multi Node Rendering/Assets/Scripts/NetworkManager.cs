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


    // Singleton instance
    private static NetworkManager instance;

    // Current connection state
    private ConnectionState state;



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
        set { state = value; }
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
        state = ConnectionState.NOT_CONNECTED;
    }




    // Update is called once per frame
    void Update()
    {

    }
}
