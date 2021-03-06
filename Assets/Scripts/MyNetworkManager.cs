﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

/// <summary>
/// An extended NetworkManager to allow increased control of how the server behaves
/// </summary>
public class MyNetworkManager : NetworkManager {
    public delegate void PlayerConnect();
    [SyncEvent]
    public event PlayerConnect EventConnectPlayer;

    public delegate void PlayerDisconnect();
    [SyncEvent]
    public event PlayerDisconnect EventDisconnectPlayer;

    public void SendConnectPlayerEvent() {
        if (EventConnectPlayer != null)
            EventConnectPlayer();
    }
    public void SendDisconnectPlayerEvent() {
        if (EventDisconnectPlayer != null)
            EventDisconnectPlayer();
    }

    int playerNumber = 0;

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        playerNumber++;
        var player = (GameObject)GameObject.Instantiate(playerPrefab, GetSpawnPosition(), Quaternion.identity); //Default implementation
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId); //Default implementation

        try { ScoreManager SM = GetComponentInChildren<ScoreManager>();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
                if (conn.clientOwnedObjects.Contains(go.GetComponent<NetworkIdentity>().netId)) {
                    go.GetComponent<EventManager>().SendScoreEvent(SM.teamOneScore, SM.teamTwoScore);
                    go.GetComponent<PlayerStats>().RpcServerInitTime(Network.time - SM.initServerTime);
                    break;
                }
        } catch { Debug.Log("No Score Manager found!"); }

        // Do the mapHandler code only when on the correct scene
        //if (onlineScene.Equals("'stinaScene_foolingaroundwithCircles")) {
        if (onlineScene.Equals("HenrikScene")) {
            mapCreator MC = GameObject.Find("mapHandler").GetComponent<mapCreator>();
            if (playerNumber == 2) {
                GetComponentInChildren<ScoreManager>().sinkTimer = (float)Network.time; // Only to be done once
                MC.SinkingSyncing(GetComponentInChildren<ScoreManager>().sinkTimer);
            }
            MC.playerConnected();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player")) 
                go.GetComponent<PlayerStats>().GenerateTerrain();
                //go.GetComponent<PlayerStats>().GenerateTerrain(GetComponentInChildren<ScoreManager>().sinkTimer);
        }
        StartCoroutine(PlayerJoined());
    }

    IEnumerator PlayerJoined() {
        yield return new WaitForSeconds(2);
        SendConnectPlayerEvent();
    }

    /// <summary>
    /// Method for determining a (re)spawn position taken from the startPositions list in NetworkManager
    /// </summary>
    /// <returns>A Vector3 position where players can spawn</returns>
    public Vector3 GetSpawnPosition() {
        if (startPositions.Count <= 0)
            return new Vector3(0, 0, 0);
        int spawn = 0;
        switch (playerSpawnMethod) { // could be changed to being team-based
            case PlayerSpawnMethod.RoundRobin:
                spawn = playerNumber % startPositions.Count;
                break;
            case PlayerSpawnMethod.Random:
                spawn = Random.Range(0, startPositions.Count);
                break;
        }
        return startPositions[spawn].position;
    }

    /// <summary>
    /// Determines what will happen when someone disconnects (server-side)
    /// 
    /// Overriden to keep track of the Player count across the teams, and (hopefully) sync 
    /// the sizes of active players according to the total team size 
    /// (and not continuously shink them down whenever someone connects-disconnects)
    /// </summary>
    /// <param name="conn"></param>
    public override void OnServerDisconnect(NetworkConnection conn) {
        playerNumber--;
        ScoreManager SM = GetComponentInChildren<ScoreManager>();
        int teamOne = 0;
        int teamTwo = 0;
        int players = 0;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player")) {
            if (conn.clientOwnedObjects.Contains(go.GetComponent<NetworkIdentity>().netId)) {
                // Get the stats of the guy who left (and remove him from the players List in SM
                SM.players.Remove(go.transform);
                continue;
            }
            PlayerStats ps = go.GetComponent<PlayerStats>();
            if (ps.team == 1) teamOne++;
            if (ps.team == 2) teamTwo++;
            players++;
        }
        Debug.Log("Previous T1: " + SM.teamOne + ", Updated T1: " + teamOne);
        Debug.Log("Previous T2: " + SM.teamTwo + ", Updated T2: " + teamTwo);
        // Update the maxHealth on players on the team that changed sizes
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player")) {
            int team = (SM.teamOne != teamOne ? 1 : 2);
            if (conn.clientOwnedObjects.Contains(go.GetComponent<NetworkIdentity>().netId)) 
                continue;
            PlayerStats ps = go.GetComponent<PlayerStats>();
            ps.changeMaxHealth = ps.team == team ? true : false;
        }

        SM.teamOne = teamOne;
        SM.teamTwo = teamTwo;
        if (players != playerNumber)
            Debug.Log("Player count mismatch; actual count: " + players);

        SendDisconnectPlayerEvent();
        base.OnServerDisconnect(conn);
    }

    public void CheckTeamSizes() {
        ScoreManager SM = GetComponentInChildren<ScoreManager>();
        int teamOne = 0;
        int teamTwo = 0;
        int players = 0;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player")) {
            PlayerStats ps = go.GetComponent<PlayerStats>();
            if (ps.team == 1) teamOne++;
            if (ps.team == 2) teamTwo++;
            players++;
        }
        Debug.Log("Previous T1: " + SM.teamOne + ", Updated T1: " + teamOne);
        Debug.Log("Previous T2: " + SM.teamTwo + ", Updated T2: " + teamTwo);
        // Update the maxHealth on players on the team that changed sizes
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player")) {
            int team = (SM.teamOne != teamOne ? 1 : 2);
            PlayerStats ps = go.GetComponent<PlayerStats>();
            ps.changeMaxHealth = ps.team == team ? true : false;
        }

        SM.teamOne = teamOne;
        SM.teamTwo = teamTwo;
    }
}