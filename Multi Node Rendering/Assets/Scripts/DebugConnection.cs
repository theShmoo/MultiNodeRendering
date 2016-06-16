using UnityEngine;
using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

/// <summary>
/// Can be used to debug a network connection
/// </summary>
class DebugConnection : NetworkConnection
{
    /// <summary>
    /// Is called when new data is received.
    /// </summary>
    /// <param name="bytes">the received data</param>
    /// <param name="numBytes">the number of bytes of the data</param>
    /// <param name="channelId">the channel of the transportation</param>
    public override void TransportRecieve(byte[] bytes, int numBytes, int channelId)
    {
        StringBuilder msg = new StringBuilder();
        for (int i = 0; i < numBytes; i++)
        {
            var s = String.Format("{0:X2}", bytes[i]);
            msg.Append(s);
            if (i > 50) break;
        }
        UnityEngine.Debug.LogError("TransportRecieve h:" + hostId + " con:" + connectionId + " bytes:" + numBytes + " " + msg);

        HandleBytes(bytes, numBytes, channelId);
    }
}