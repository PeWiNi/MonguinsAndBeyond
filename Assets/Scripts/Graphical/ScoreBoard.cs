using UnityEngine;
using System.Collections;

public class ScoreBoard : MonoBehaviour {
    public bool showScoreBoard;
    [SerializeField]
    public int offsetX;
    [SerializeField]
    public int offsetY;
    
    public int[] teams;
    public int[] kills;
    public int[] deaths;
    public float[] score;

    void OnGUI() {
        if (!showScoreBoard) {
            return;
        }

        int xpos = 10 + offsetX;
        int ypos = 100 + offsetY;
        int spacing = 24;
        float team1Score = 0;
        float team2Score = 0;
        
        for(int i = 0; i < teams.Length; i++) {
            string write = string.Format("Player {0} (team {1}): Kills: {2:00}, Deaths: {3:00}, Score: {4:000.00}", i, teams[i], kills[i], deaths[i], score[i]);
            if (teams[i] == 1) team1Score += score[i];
            if (teams[i] == 2) team2Score += score[i];
            GUI.TextArea(new Rect(xpos, ypos, 340, 20), write);
            ypos += spacing;
        }
        GUI.TextArea(new Rect(xpos, ypos, 340, 20), "Team 1 Score: " + team1Score);
        ypos += spacing;
        GUI.TextArea(new Rect(xpos, ypos, 340, 20), "Team 2 Score: " + team2Score);
    }
}
