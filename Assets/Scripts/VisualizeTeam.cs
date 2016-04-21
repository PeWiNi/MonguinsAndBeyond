using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class VisualizeTeam : NetworkBehaviour {
    [SerializeField]
    GameObject fish;
    [SerializeField]
    GameObject banana;

    public void ToggleForeheadItem(int team, bool show = true) {
        banana.SetActive(show ? team == 1 : false);
        fish.SetActive(show ? team == 2 : false);
    }
}
