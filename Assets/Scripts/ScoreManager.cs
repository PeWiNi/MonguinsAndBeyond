using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Class for keeping track of teamsizes 
/// </summary>
public class ScoreManager : NetworkBehaviour {
    public float initialHealthPool = 1000;
    public int teamOne = 0;
    public int teamTwo = 0;
    public float teamOneDeathCount = 0;
    public float teamTwoDeathCount = 0;

    /// <summary>
    /// Tallies the count of players for each team, also assigns a team-number if you did not select one yourself
    /// </summary>
    /// <param name="player">PlayerStats of the the newly connected player</param>
    public void TeamSelection(PlayerStats player) {
        int team = player.team; // OLD COMMENT useless, team is not set in the prefab....
        //if (team == 0) { team = playerNumber % 2 == 1 ? 1 : 2; }
        if (team == 0) {
            team = teamOne <= teamTwo ? 1 : 2;
            player.team = team;
        }
        if (team == 1) { teamOne++; }
        if (team == 2) { teamTwo++; }
    }

    /// <summary>
    /// Method triggered whenever someone dies
    /// </summary>
    /// <param name="team">Team number of player killed</param>
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

    /// <summary>
    /// Method containing the formula used to determine the health of players in regards to team size
    /// 
    /// --Formula created 08-02-2016--
    /// </summary>
    /// <param name="numberOfPlayers">The number of players on a team</param>
    /// <returns>Calculated maxHealth for players of the team</returns>
    public float compositeHealthFormula(int numberOfPlayers) {
        float health = initialHealthPool;
        for (int i = 1; i < numberOfPlayers; i++) {
            health *= 0.875f;
        }
        if (numberOfPlayers > 9) {
            health += (numberOfPlayers - 7) * 100;
            health /= numberOfPlayers - 8;
        }
        print("Number of players is: " + numberOfPlayers);
        print("Calculated Health is: " + health);
        return health;
    }

    /// <summary>
    /// Send a status update on deaths across teams to clients
    /// </summary>
    [ClientRpc]
    void RpcDeathCount() {
        Debug.Log(string.Format("Team Banana has been killed: {0} \nTeam Fish has been killed: {1}", teamOneDeathCount, teamTwoDeathCount));
    }
}
