using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Trap_Spikes : Trap
{
    [SyncVar]
    public float impactDamage = 0.06f;//When a player first enter
    [SyncVar]
    public float ticDamage = 0.005f;

    public void SetDmgModifier(float dmgModifier) {
        impactDamage *= dmgModifier;
        ticDamage *= dmgModifier;
    }

    void OnTriggerStay(Collider col)
    {
        if (col.tag == "Player")
        {
            col.transform.GetComponent<PlayerBehaviour>().TakeDamageWhileMoving(ticDamage, owner.transform);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            col.transform.GetComponent<PlayerBehaviour>().lastPosition = col.transform.position;
            col.transform.GetComponent<PlayerBehaviour>().enterTime = Network.time;
            col.transform.GetComponent<PlayerStats>().TakeDmg(col.transform.GetComponent<PlayerStats>().maxHealth * impactDamage, owner.transform);
        }
    }

    void OnTriggerExit(Collider _col) {
        if (_col.tag == "Player") {
            //_col.gameObject.GetComponent<Animator>().SetBool("AffectedBySpikeTrap", false);
            DecrementTrigger();
        }
    }
}
