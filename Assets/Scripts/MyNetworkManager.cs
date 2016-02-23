using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class MyNetworkManager : NetworkManager {
    int playerNumber = 0;

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        playerNumber++; // currently determines the team the player is assigned to - unfair if every other player leaves :D
        int spawn = 0;
        switch (playerSpawnMethod) { // could be changed to being team-based
            case PlayerSpawnMethod.RoundRobin:
                spawn = playerNumber % startPositions.Count;
                break;
            case PlayerSpawnMethod.Random:
                spawn = Random.Range(0, startPositions.Count);
                break;
        }
        Vector3 position = new Vector3(0, 0, 0);
        if (startPositions.Count > 0)
            position = startPositions[spawn].position;

        var player = (GameObject)GameObject.Instantiate(playerPrefab, position, Quaternion.identity); //Default implementation
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId); //Default implementation
        if(onlineScene.Equals("'stinaScene_foolingaroundwithCircles")) {
            GameObject.Find("mapHandler").GetComponent<mapCreator>().playerConnected();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
                go.GetComponent<PlayerStats>().GenerateTerrain();
        }
    }

    public Vector3 GetRespawnPosition() {
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

    public override void OnServerDisconnect(NetworkConnection conn) {
        playerNumber--;
        ScoreManager SM = GetComponentInChildren<ScoreManager>();
        int teamOne = 0;
        int teamTwo = 0;
        int players = 0;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player")) {
            if (conn.clientOwnedObjects.Contains(go.GetComponent<NetworkIdentity>().netId)) 
                continue;
            PlayerStats ps = go.GetComponent<PlayerStats>();
            if (ps.team == 1) teamOne++;
            if (ps.team == 2) teamTwo++;
            players++;
        }
        SM.teamOne = teamOne;
        SM.teamTwo = teamTwo;
        if (players != playerNumber)
            Debug.Log("Player count mismatch; actual count: " + players);
        base.OnServerDisconnect(conn);
    }
}