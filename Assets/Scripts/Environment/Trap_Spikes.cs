using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Trap_Spikes : Trap
{
    public float impactDamage = 0.06f;//When a player first enter
    public float ticDamage = 0.005f;

    void OnTriggerStay(Collider col)
    {
        if (col.tag == "Player")
        {
            col.transform.GetComponent<PlayerBehaviour>().TakeDamageWhileMoving(ticDamage);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            col.transform.GetComponent<PlayerBehaviour>().lastPosition = col.transform.position;
            col.transform.GetComponent<PlayerBehaviour>().enterTime = Network.time;
            col.transform.GetComponent<PlayerStats>().TakeDmg(col.transform.GetComponent<PlayerStats>().maxHealth * impactDamage);
        }
    }

    void OnTriggerExit(Collider _col) {
        if (_col.tag == "Player") {
            DecrementTrigger();
        }
    }
}
