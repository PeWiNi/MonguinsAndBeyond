using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class HealSelf : Ability {
    
    public override void Trigger() {
        GetComponentInParent<PlayerStats>().CmdHealing(100); // Trigger healing on server
    }
}
