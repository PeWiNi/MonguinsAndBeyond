using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Trap_Spikes : Trap
{
    public float impactDamage = 50f;//When a player first enter
    public float dotDamage = 5f;

    void OnTriggerStay(Collider col)
    {
        if (col.tag == "Player")
        {
            col.transform.GetComponent<PlayerBehaviour>().TakeDamageWhileMoving(dotDamage);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            col.transform.GetComponent<PlayerBehaviour>().lastPosition = col.transform.position;
            col.transform.GetComponent<PlayerStats>().TakeDmg(impactDamage);
        }
    }

    void OnTriggerExit(Collider _col) {
        if (_col.tag == "Player") {
            DecrementTrigger();
        }
    }
}
