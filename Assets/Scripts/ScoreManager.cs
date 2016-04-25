using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

/// <summary>
/// Class for keeping track of teamsizes 
/// </summary>
public class ScoreManager : NetworkBehaviour {
    public double initServerTime;

    public float initialHealthPool = 1000;
    public int teamOne = 0;
    public int teamTwo = 0;
    public float teamOneDeathCount = 0;
    public float teamTwoDeathCount = 0;

    public List<Transform> players = new List<Transform>();
    public List<PlayerStats> legacyPlayers = new List<PlayerStats>();

    public float sinkTimer;

    void Start() {
        initServerTime = Network.time;
    }

    /// <summary>
    /// Tallies the count of players for each team, also assigns a team-number if you did not select one yourself
    /// </summary>
    /// <param name="player">PlayerStats of the the newly connected player</param>
    public void TeamSelection(PlayerStats player) {
        players.Add(player.transform);
        int team = player.team;
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
    /// <param name="killer">Player who killed the fella</param>
    public void CountDeaths(int team, PlayerStats killer) {
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
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            go.GetComponent<EventManager>().SendScoreEvent(teamOneDeathCount, teamTwoDeathCount);
        if(killer) {
            killer.score += GetKillScore(killer);
            killer.GetComponent<EventManager>().SendPersonalScore(killer.score - GetDeathsScore(killer));
        } else {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
                go.GetComponent<EventManager>().SendPersonalScore(go.GetComponent<PlayerStats>().score - GetDeathsScore(go.GetComponent<PlayerStats>()));
        }
    }

    public float GetKillScore(PlayerStats player) {
        float score = 0;
        score = 4 / (player.team == 1 ? teamTwo : teamOne);
        return score;
    }

    public float GetDeathsScore(PlayerStats player) {
        float score = 0;
        score = player.deaths * 2;
        return score;
    }

    public PlayerStats[] GetScoreBoard() {
        PlayerStats[] ps = new PlayerStats[players.Count + legacyPlayers.Count];
        int i = 0;
        foreach (Transform t in players)
            ps[i++] = t.GetComponent<PlayerStats>();
        foreach (PlayerStats lp in legacyPlayers)
            ps[i++] = lp.GetComponent<PlayerStats>();
        return ps;
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
