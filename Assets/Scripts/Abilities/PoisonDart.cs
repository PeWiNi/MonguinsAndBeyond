using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// 
/// 
/// Throw poison - ranged ability, slows the enemy at 0,5*speed and deals 0.5% damage*max health over 3 sec (1.5% in total). Range from 5 to 30 distance units. No CD; takes 1 sec to cast and requires poisonous herbs @12/04/16 'stina: no slow 1.5% dmg whatever 3 times
/// </summary>
public class PoisonDart : NetworkBehaviour {
    Vector3 Destination;
    float tickDamage;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void setup(float damageTick, Vector3 endPos) {
        tickDamage = damageTick;
        Destination = endPos;
    }

    [Command]
    internal void CmdDamagePlayerOverTime(GameObject player, float damageTick, float duration) {
        player.GetComponent<PlayerStats>().BadBerry(damageTick, duration);
    }
    [Command]
    internal void CmdSlowPlayer(GameObject player, float duration) {
        player.GetComponent<PlayerStats>().Slow(true, duration);
    }
}
