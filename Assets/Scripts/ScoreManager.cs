using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ScoreManager : NetworkBehaviour {
    public float teamOneDeathCount = 0;
    public float teamTwoDeathCount = 0;

    public void CountDeaths(int team) {
        switch (team) {
            case (1):
                teamOneDeathCount++;
                break;
            case (2):
                teamOneDeathCount++;
                break;
            default:
                break;
        }
        RpcDeathCount();
    }

    [ClientRpc]
    void RpcDeathCount() {
        Debug.Log(string.Format("Team Banana has been killed: {0} \nTeam Fish has been killed: {1}", teamOneDeathCount, teamTwoDeathCount));
    }
}
