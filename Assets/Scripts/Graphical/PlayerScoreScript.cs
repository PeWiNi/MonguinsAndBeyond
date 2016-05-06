using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerScoreScript : MonoBehaviour {

    [SerializeField]
    Text Player;
    [SerializeField]
    Text Kills;
    [SerializeField]
    Text Deaths;
    [SerializeField]
    Text Score;

    [SerializeField]
    Color Dark;
    [SerializeField]
    Color Light;

    public void SetParams(string name, string kills, string deaths, string score, int darkColor) {
        Player.text = name;
        Kills.text = kills;
        Deaths.text = deaths;
        Score.text = score;
        GetComponent<Image>().color = darkColor == 1 ? Dark : darkColor == 0 ? Light : Color.magenta * new Color(1, 1, 1, .5f);
    }
}
