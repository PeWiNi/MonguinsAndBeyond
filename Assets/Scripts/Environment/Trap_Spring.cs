using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Trap_Spring : Trap
{
    public GameObject player;
    [Tooltip("The force applied to the Player")]
    public float thrust = 0f;
    [Tooltip("The BoxCollider resembling the platform of the spring trap")]
    public BoxCollider springplatform = null;
    [Tooltip("The landing spot which players will be thrown towards")]
    public Vector3 theLandingSpotposition;
    [Tooltip("The angle of the rubber tree")]
    public float shootingAngle;
    [Tooltip("The range for which players will be thrown")]
    public float shootingRange;

    // Use this for initialization
    void Start()
    {
        if (springplatform == null)
            springplatform = GetComponentInChildren<BoxCollider>();
    }

    void Update()
    {

    }

    Vector3 CircleStuff(Vector3 center, float radius, float distance, float min = 0, float max = 10)
    {
        var ang = Mathf.Clamp(distance, 0, 10) / 10 * -90;
        //ang += 90;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }

    //public float Remap(float value, float from1, float to1, float from2, float to2)
    /// <summary>
    /// Remap the distance.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="from1"></param>
    /// <param name="to1"></param>
    /// <returns></returns>
    public float Remap(float value, float from1, float to1)
    {
        float from2 = 2f;//C
        float to2 = 5 * from2;//D
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    //float DistanceMapping(float angle)
    //{
    //    float max = 0.8f;
    //    float min = 1f;
    //    float maxDistance = 30f;
    //    float minDistance = maxDistance / 5f;// 1/5 of the maxDistance.
    //    return (angle - min) / (minDistance - min) * (maxDistance - max) + max;
    //}

    void OnTriggerEnter(Collider _collider)
    {
        if (_collider.tag == "Player")
        {
            //We search for all Players in the triggerRadius to determine who should be thrown.
            Collider[] playersWithinRange = Physics.OverlapBox(springplatform.gameObject.transform.position, springplatform.bounds.extents / 2);
            foreach (Collider col in playersWithinRange)
            {
                if (col.tag == "Player")
                {
                    thrust = Mathf.Sqrt((shootingRange * Physics.gravity.magnitude) / Mathf.Sin(2f * shootingAngle * Mathf.Deg2Rad));//Determine the launch velocity.
                    print("Thrust = " + thrust);
                    //_collider.gameObject.GetComponent<PlayerBehaviour>().PlayerThrown(thrust, theLandingSpotposition, shootingAngle);
                    _collider.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 10f, ForceMode.Impulse);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (springplatform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(springplatform.bounds.center, springplatform.bounds.extents / 2);
        }
    }
}
