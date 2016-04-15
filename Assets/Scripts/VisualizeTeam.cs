using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class VisualizeTeam : NetworkBehaviour {
    [SerializeField]
    GameObject fish;
    [SerializeField]
    GameObject banana;

    public void ToggleForeheadItem(int team, bool show = true) {
        banana.SetActive(team == 1 ? true : false);
        fish.SetActive(team == 2 ? true : false);
    }
}
