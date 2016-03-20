using UnityEngine;
using System.Collections;

public class Trap_VineTree : Trap
{
    [Tooltip("The force applied to the Player")]
    public float thrust = 50f;
    [Tooltip("The SphereCollider used for determine the triggerRadius area")]
    public SphereCollider crownTriggerCollider = null;
    [Tooltip("The Direction which players will be thrown")]
    public Vector3 direction;

    // Use this for initialization
    void Start()
    {
        if (crownTriggerCollider == null)
            crownTriggerCollider = GetComponentInChildren<SphereCollider>();
    }

    void OnTriggerEnter(Collider _collider)
    {
        if (_collider.transform.tag == "Player")
        {
            print("HEJSA !");
            //We search for all Players in the triggerRadius to determine who should be thrown.
            Collider[] playersWithinRange = Physics.OverlapSphere(transform.position, 2f);
            foreach (Collider col in playersWithinRange)
            {
                print("col.gameObject = " + col.gameObject);
                if (col.transform.tag == "Player")
                {
                    print("SHIT!");
                    //_collider.gameObject.GetComponent<Throw>().PlayerThrown(thrust, transform.position, attachedCollider.radius);//Explosion
                    //_collider.gameObject.GetComponent<Throw>().PlayerThrown(thrust);//Force
                    _collider.gameObject.GetComponent<PlayerBehaviour>().PlayerThrown(thrust);//Force
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (crownTriggerCollider != null)
        {
            Gizmos.color = Color.blue;
            //Gizmos.DrawWireSphere(crownTriggerCollider.bounds.center, crownTriggerCollider.radius + crownTriggerCollider.transform.localScale.x * 1.5f);
            Gizmos.DrawWireSphere(crownTriggerCollider.bounds.center, 2f);
        }
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }
}
