using UnityEngine;
using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

class DebugConnection : NetworkConnection
{
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