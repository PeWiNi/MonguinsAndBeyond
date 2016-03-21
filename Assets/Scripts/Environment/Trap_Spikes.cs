using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Trap_Spikes : Trap
{
    public float impactDamage = 100f;//When a player first enter
    public float dotDamage = 10f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

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
}
