using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class EventManager : NetworkBehaviour {
    public delegate void ScoreChange(float team1, float team2);
    [SyncEvent]
    public static event ScoreChange EventScoreChange;

    public static float amberStunTime = 12;

    [ClientRpc]
    public void RpcSendScoreEvent(float team1, float team2) {
        if (EventScoreChange != null)
            EventScoreChange(team1, team2);
    }
}
