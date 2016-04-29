using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class EventManager : NetworkBehaviour {
    public delegate void ScoreChange(float team1, float team2);
    [SyncEvent]
    public event ScoreChange EventScoreChange;

    public delegate void ScoreBoard(string[] names, int[] team, int[] kills, int[] deaths, float[] score, int[] teamKills, int[] teamDeaths, float[] teamScore);
    [SyncEvent]
    public event ScoreBoard EventScoreBoard;

    public static float amberStunTime = 12;

    public void SendScoreEvent(float team1, float team2) {
        if (EventScoreChange != null)
            EventScoreChange(team1, team2);
    }

    public void SendScoreBoardEvent(string[] names, int[] team, int[] kills, int[] deaths, float[] score, int[] teamKills, int[] teamDeaths, float[] teamScore) {
        if (EventScoreBoard != null)
            EventScoreBoard(names, team, kills, deaths, score, teamKills, teamDeaths, teamScore);
    }
}
