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
    public int teamOneDeathCount = 0;
    public int teamTwoDeathCount = 0;
    public int teamOneKillCount = 0;
    public int teamTwoKillCount = 0;
    public float teamOneScore = 0;
    public float teamTwoScore = 0;

    public List<Transform> players = new List<Transform>();

    public float sinkTimer;

    void Start() {
        initServerTime = Network.time;
        Invoke("EndGame", 900);
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
    /// <param name="victim">Player that got killed</param>
    /// <param name="killer">Player who killed the fella</param>
    public void CountDeaths(PlayerStats victim, PlayerStats killer) {
        switch (victim.team) {
            case (1):
                teamOneDeathCount++;
                if (killer)
                    teamTwoKillCount++;
                teamOneScore -= GetDeathsScore(victim);
                break;
            case (2):
                teamTwoDeathCount++;
                if (killer)
                    teamOneKillCount++;
                teamTwoScore -= GetDeathsScore(victim);
                break;
            default:
                break;
        } victim.score -= GetDeathsScore(victim);
        if (killer) {
            float score = GetKillScore(victim);
            killer.score += score;
            if (killer.team == 1) teamOneScore += score;
            else if (killer.team == 2) teamTwoScore += score;
        }
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            go.GetComponent<EventManager>().SendScoreEvent(teamOneScore, teamTwoScore);
    }

    public float GetKillScore(PlayerStats victim) {
        return victim.maxHealth / 250;
    }

    public float GetDeathsScore(PlayerStats player) {
        return player.maxHealth / 500;
    }

    public PlayerStats[] GetScoreBoard() {
        PlayerStats[] ps = new PlayerStats[players.Count];
        int i = 0;
        foreach (Transform t in players)
            ps[i++] = t.GetComponent<PlayerStats>();
        return ps;
    }

    void EndGame() {
        PlayerStats[] ps = GetScoreBoard();
        string[] names = new string[ps.Length];
        int[] team = new int[ps.Length];
        int[] kills = new int[ps.Length];
        int[] deaths = new int[ps.Length];
        float[] score = new float[ps.Length];
        int[] teamKills = new int[2] { teamOneKillCount, teamTwoKillCount };
        int[] teamDeaths = new int[2] { teamOneDeathCount, teamTwoDeathCount };
        float[] teamScore = new float[2] { teamOneScore, teamTwoScore };
        for (int i = 0; i < ps.Length; i++) {
            if (!ps[i]) continue;
            names[i] = ps[i].playerName;
            team[i] = ps[i].team;
            kills[i] = ps[i].kills;
            deaths[i] = ps[i].deaths;
            score[i] = ps[i].score;
        }
        GameObject.FindGameObjectWithTag("Player").GetComponent<EventManager>().SendScoreBoardEvent(names, team, kills, deaths, score, teamKills, teamDeaths, teamScore);
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player")) {
            go.GetComponent<EventManager>().SendEndGame();
            go.GetComponent<PlayerStats>().Stun(float.MaxValue);
        }
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
}
