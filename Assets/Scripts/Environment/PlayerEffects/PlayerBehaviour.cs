using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerBehaviour : NetworkBehaviour
{
    //Catapult variables.
    public bool isThrown = false;
    float thrust = 0f;
    Vector3 origin = Vector3.zero;//Angle should between 30-60 Degrees based on player current position.
    float radius = 0f;

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
            Vector3 randomDirection = new Vector3(transform.position.x + Random.Range(30f, 60f), transform.position.y, transform.position.z + Random.Range(30f, 60f));
            //transform.GetComponent<Rigidbody>().AddExplosionForce(thrust, origin, radius, thrust / 10, ForceMode.Impulse);//Explosion
            transform.GetComponent<Rigidbody>().AddForce(transform.up * thrust + randomDirection, ForceMode.Impulse);//Force
            isThrown = false;

            //float d = Vector3.Distance(destination, rb.transform.position); //Get distance between the current position and the destination
            //float tsqr = timeToReach * timeToReach; //Get the time to reach the destination squared
            //float m = rb.mass; //Get the mass of the object to be tossed
            //float f = (float)(d / ((.5) * tsqr)) * m; //d = v*t + (1/2)at^2 substituting a for f/m and solving for f
            //rb.AddForce(f * Vector3.forward); //Apply force to rigidbody
        }
    }

    /// <summary>
    /// Player was thrown #Catapult.
    /// </summary>
    /// <param name="thrust"></param>
    /// <param name="direction"></param>
    /// <param name="triggerRadius"></param>
    //public void PlayerThrown(float thrust, Vector3 origin, float triggerRadius)//Explosion
    public void PlayerThrown(float thrust)//Force
    {
        if (!isLocalPlayer || isServer)
            return;
        print("Holy cow!");
        //this.origin = origin;
        this.thrust = thrust;
        //this.radius = triggerRadius;
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
