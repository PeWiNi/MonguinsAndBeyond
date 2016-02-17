using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ScoreManager : NetworkBehaviour {
    public float initialHealthPool = 1000;
    public int teamOne = 0;
    public int teamTwo = 0;
    public float teamOneDeathCount = 0;
    public float teamTwoDeathCount = 0;

    public void TeamSelection(PlayerStats player) {
        int team = player.team; //useless, team is not set in the prefab....
        //if (team == 0) { team = playerNumber % 2 == 1 ? 1 : 2; }
        if (team == 0) {
            team = teamOne <= teamTwo ? 1 : 2;
            player.team = team;
        }
        if (team == 1) { teamOne++; }
        if (team == 2) { teamTwo++; }
    }

    public void CountDeaths(int team) {
        switch (team) {
            case (1):
                teamOneDeathCount++;
                break;
            case (2):
                teamTwoDeathCount++;
                break;
            default:
                break;
        }
        RpcDeathCount();
    }

    public float compositeHealthFormula(int numberOfPlayers) {
        float health = initialHealthPool;
        for (int i = 1; i < numberOfPlayers; i++) {
            health *= 0.875f;
        }
        if (numberOfPlayers > 9) {
            health += (numberOfPlayers - 7) * 100;
            health /= numberOfPlayers - 8;
        }

        return health;
    }

    [ClientRpc]
    void RpcDeathCount() {
        Debug.Log(string.Format("Team Banana has been killed: {0} \nTeam Fish has been killed: {1}", teamOneDeathCount, teamTwoDeathCount));
    }
}
