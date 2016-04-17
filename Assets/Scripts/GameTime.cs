using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GameTime : NetworkBehaviour {
    public double time;
    float deltaTime;

    void Start() {
        if (!isServer) {
            CmdSyncDeltaTime();
        } else {
            time = Network.time;
        }
    }

    // Update is called once per frame
    void Update() {
        if (isServer) time = Network.time; //truthfully the server can always reference network.time while clients can reference netx.time, but this makes code a bit easier
        else time = Network.time + deltaTime; //cause network.time is synched, just starts at different points, so just add the difference
    }

    [Command]
    void CmdSyncDeltaTime() {
        RpcSetDeltaTime(Network.time);
    }

    //get the difference in time from local network.time to the server's time.
    [ClientRpc]
    void RpcSetDeltaTime(double newTime) {
        deltaTime = (float)(newTime - Network.time);
        Debug.Log("Delta from network.time to server's network.time is " + deltaTime);
    }
}