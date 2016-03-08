using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Throw : NetworkBehaviour
{
    public bool isThrown = false;
    float thrust = 0f;
    Vector3 origin = Vector3.zero;//Angle should between 30-60 Degrees based on player current position.
    float radius = 0f;

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
        }
    }

    /// <summary>
    /// Throws the player away!.
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

    [Command]
    void CmdStun(int effectDuration)
    {
        transform.GetComponent<PlayerStats>().Stun(effectDuration);
    }
}
