using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class MyNetworkManager : NetworkManager {
    int playerNumber = 0;
    int teamOne = 0;
    int teamTwo = 0;

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
        #region TeamSelection
        int team = player.GetComponent<PlayerStats>().team;
        //if (team == 0) { team = playerNumber % 2 == 1 ? 1 : 2; }
        if (team == 0) {
            team = teamOne <= teamTwo ? 1 : 2;
            player.GetComponent<PlayerStats>().team = team;
        }
        if (team == 1) { teamOne++; } 
        if (team == 2) { teamTwo++; }
        #endregion

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
}