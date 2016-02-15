using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class HealSelf : Ability {
    
    public override double Trigger() {
        GetComponentInParent<PlayerStats>().CmdHealing(100); // Trigger healing on server
        return castTime;
    }
}
