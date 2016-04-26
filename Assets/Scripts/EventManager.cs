using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class EventManager : NetworkBehaviour {
    public delegate void ScoreChange(float team1, float team2);
    [SyncEvent]
    public event ScoreChange EventScoreChange;

    public delegate void ScoreBoard(int[] team, int[] kills, int[] deaths, float[] score);
    [SyncEvent]
    public event ScoreBoard EventScoreBoard;

    public delegate void PersonalScore(float score);
    [SyncEvent]
    public event PersonalScore EventPersonalScore;

    public static float amberStunTime = 12;

    public void SendScoreEvent(float team1, float team2) {
        if (EventScoreChange != null)
            EventScoreChange(team1, team2);
    }

    public void SendScoreBoardEvent(int[] team, int[] kills, int[] deaths, float[] score) {
        if (EventScoreBoard != null)
            EventScoreBoard(team, kills, deaths, score);
    }

    public void SendPersonalScore(float score) {
        if (EventPersonalScore != null)
            EventPersonalScore(score);
    }
}
