using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerBehaviour : NetworkBehaviour
{
    //Catapult variables.
    public bool isThrown = false;
    float thrust = 0f;
    float shootingAngle = 0f;
    Vector3 targetLandingSpot;

    //Slip variables.
    public bool isSlipping = false;
    //float thrust = 0f;

    //Spike variables
    public bool isMoving = false;
    public Vector3 lastPosition;
    public float distanceTravelled = 0f;

    // Use this for initialization
    void Start()
    {
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (isThrown)
        {
            print("WHY NOOO!");
            //Vector3 randomDirection = new Vector3(transform.position.x + Random.Range(30f, 60f), transform.position.y, transform.position.z + Random.Range(30f, 60f));
            //transform.GetComponent<Rigidbody>().AddForce(transform.up * thrust + randomDirection, ForceMode.Impulse);
            if (!float.IsNaN(BallisticVel(targetLandingSpot, shootingAngle).x) && !float.IsNaN(BallisticVel(targetLandingSpot, shootingAngle).y) && !float.IsNaN(BallisticVel(targetLandingSpot, shootingAngle).z))
                transform.GetComponent<Rigidbody>().velocity = BallisticVel(targetLandingSpot, shootingAngle);
            isThrown = false;
        }
    }

    /// <summary>
    /// Creates a balistic velocity for the player.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public Vector3 BallisticVel(Vector3 target, float angle)
    {
        Vector3 dir = target - transform.position; // get target direction 
        float h = dir.y; // get height difference 
        dir.y = 0; // retain only the horizontal direction 
        float dist = dir.magnitude; // get horizontal distance 
        float a = angle * Mathf.Deg2Rad; // convert angle to radians 
        dir.y = dist * Mathf.Tan(a); // set dir to the elevation angle 
        dist += h / Mathf.Tan(a); // correct for small height differences 
        var vel = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));// calculate the velocity magnitude 
        return vel * dir.normalized;
    }

    /// <summary>
    /// Player was thrown #Catapult.
    /// </summary>
    /// <param name="thrust"></param>
    public void PlayerThrown(float thrust, Vector3 landingSpot, float angle)
    {
        if (!isLocalPlayer || isServer)
            return;
        print("Holy cow!");
        this.thrust = thrust;
        this.targetLandingSpot = landingSpot;
        this.shootingAngle = angle;
        isThrown = true;
        CmdStun(1);
    }

    /// <summary>
    /// Player slipped
    /// </summary>
    /// <param name="effectDuration"></param>
    /// <param name="thrust"></param>
    public void PlayerSlipped(int effectDuration, float thrust)
    {
        if (!isLocalPlayer || isServer)
            return;
        if (!isSlipping)
        {
            this.thrust = thrust;
            StartCoroutine(Slipping(effectDuration, thrust));
        }
    }

    IEnumerator Slipping(int effectDuration, float thrust)
    {
        isSlipping = true;
        transform.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * thrust, ForceMode.Impulse);
        CmdStun(effectDuration);
        while (effectDuration > 0)
        {
            yield return new WaitForSeconds(1f);
            effectDuration--;
        }
        isSlipping = false;
    }

    /// <summary>
    /// The player will take damage while moving inside a Spike Trap.
    /// </summary>
    /// <param name="player"></param>
    public void TakeDamageWhileMoving(float dotDamage)
    {
        distanceTravelled = Vector3.Distance(transform.position, lastPosition);
        if (distanceTravelled >= 0.1f)
        {
            lastPosition = transform.position;
            print("Moved!");
            GetComponent<PlayerStats>().TakeDmg(dotDamage);
        }
    }

    /// <summary>
    /// Checks whether or not the player has moved from their origin point using a threshold value.
    /// </summary>
    /// <param name="here"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    bool HasMoved(Vector3 here, float threshold)
    {
        bool ret = false;
        if ((lastPosition.x + threshold >= here.x &&
            lastPosition.x - threshold <= here.x) &&
            (lastPosition.z + threshold >= here.z &&
            lastPosition.z - threshold <= here.z))
            ret = true;
        return ret;
    }

    [Command]
    void CmdStun(int effectDuration)
    {
        transform.GetComponent<PlayerStats>().Stun(effectDuration);
    }
}
