using UnityEngine;
using System.Collections;

public class Trap_VineTree : Trap
{
    [Tooltip("The force applied to the Player")]
    public float thrust = 50f;
    [Tooltip("The SphereCollider used for determine the triggerRadius area")]
    public SphereCollider attachedCollider = null;

    // Use this for initialization
    void Start()
    {
        attachedCollider = GetComponent<SphereCollider>();
    }

    void OnTriggerEnter(Collider _collider)
    {
        if (_collider.transform.tag == "Player")
        {
            //We search for all Players in the triggerRadius to determine who should be thrown.
            Collider[] playersWithinRange = Physics.OverlapSphere(transform.position, attachedCollider.radius);
            foreach (Collider col in playersWithinRange)
            {
                if (col.transform.tag == "Player")
                {
                    print("SHIT!");
                    //_collider.gameObject.GetComponent<Throw>().PlayerThrown(thrust, transform.position, attachedCollider.radius);//Explosion
                    _collider.gameObject.GetComponent<Throw>().PlayerThrown(thrust);//Force
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (attachedCollider != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, attachedCollider.radius);
        }
    }
}
