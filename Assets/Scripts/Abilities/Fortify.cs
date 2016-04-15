using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
/// <summary>
/// Defender 
/// Enhancement ability
/// 
/// Fortify - temporarily increase health and resilience of the defender with 20% for 10 sec. CD:20sec
/// </summary>
public class Fortify : Ability {
    public float duration = 10;
    public float effect = 20;

    public override double Trigger() {
        CmdFortify(gameObject, duration);
        return base.Trigger();
    }

    [Command]
    void CmdFortify(GameObject player, float duration) {
        // TODO: Implement in PlayerStats
        player.GetComponent<PlayerStats>().Fortify(duration);
    }
}
