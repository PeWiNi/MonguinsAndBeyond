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
        Vector3 position = startPositions[spawn].position;
        var player = (GameObject)GameObject.Instantiate(playerPrefab, position, Quaternion.identity);
        player.GetComponent<PlayerStats>().team = playerNumber % 2 == 1 ? 1 : 2; //TODO allow players to choose teams
        //if (playerNumber == 1) { player.GetComponent<PlayerStats>().team = 1; } 
        //if (playerNumber == 2) { player.GetComponent<PlayerStats>().team = 2; }
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
}