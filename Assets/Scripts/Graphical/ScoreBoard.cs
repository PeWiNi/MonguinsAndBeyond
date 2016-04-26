using UnityEngine;
using System.Collections;

public class ScoreBoard : MonoBehaviour {
    public bool showScoreBoard;

    [SerializeField]
    Transform team1;
    [SerializeField]
    Transform team2;

    [SerializeField]
    GameObject scoreBoard;

    public int[] teams;
    public int[] kills;
    public int[] deaths;
    public float[] score;

    void Start() {
        scoreBoard.SetActive(false);
    }

    void Update() {
        if (!showScoreBoard && scoreBoard.activeSelf) {
            scoreBoard.SetActive(false);

            Clear(team1);
            Clear(team2);
        } else if (showScoreBoard && !scoreBoard.activeSelf) {
            scoreBoard.SetActive(true);

            float team1Score = 0;
            float team2Score = 0;
            for (int i = 0; i < teams.Length; i++) {
                Add("Player " + (i + 1), kills[i], deaths[i], score[i], teams[i] == 1 ? team1 : team2);
                if(teams[i] == 1) team1Score += score[i];
                if(teams[i] == 2) team2Score += score[i];
            }
            Add("TOTAL: ", 0, 0, team1Score, team1);
            Add("TOTAL: ", 0, 0, team2Score, team2);
        }
    }

    void Add(string name, int kills, int deaths, float score, Transform team) {
        GameObject ps = Instantiate(Resources.Load("Prefabs/GUI/PlayerThing"), new Vector3(), Quaternion.identity) as GameObject;
        ps.transform.parent = team.transform;
        ps.GetComponent<PlayerScoreScript>().SetParams(name, string.Format("{0:00}", kills), string.Format("{0:00}", deaths), string.Format("{0,7:000.00}", score), (team.childCount % 2) == 1);
    }

    void Clear(Transform content) {
        foreach (Transform child in content) {
            Destroy(child.gameObject);
        }
    }

    /*void OnGUI() {
        if (!showScoreBoard) {
            return;
        }

        int xpos = 10;
        int ypos = 100;
        int yposT1 = ypos;
        int yposT2 = ypos;
        int spacing = 24;
        float team1Score = 0;
        float team2Score = 0;
        
        GUI.Label(new Rect(xpos, yposT1, 340, 20), "Team 1");
        GUI.Label(new Rect(xpos + 350, yposT2, 340, 20), "Team 2");
        yposT1 += spacing;
        yposT2 += spacing;
        for (int i = 0; i < teams.Length; i++) {
            string write = string.Format("Player {0}: Kills: {1:00}, Deaths: {2:00}, Score: {3,7:000.00}", i + 1, kills[i], deaths[i], score[i]);
            if (teams[i] == 1) {
                team1Score += score[i];
                GUI.Label(new Rect(xpos, yposT1, 340, 20), write);
                yposT1 += spacing;
            }
            if (teams[i] == 2) {
                team2Score += score[i];
                GUI.Label(new Rect(xpos + 350, yposT2, 340, 20), write);
                yposT2 += spacing;
            }
        }
        GUI.Label(new Rect(xpos, yposT1, 340, 20), "Team 1 Score: " + team1Score);
        GUI.Label(new Rect(xpos + 350, yposT2, 340, 20), "Team 2 Score: " + team2Score);
    }*/
}
