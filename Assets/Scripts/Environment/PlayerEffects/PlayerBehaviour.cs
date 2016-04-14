using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerBehaviour : NetworkBehaviour
{
    //Catapult variables.
    public bool isThrown = false;
    public float thrust = 0f;//Will be calculated.
    public float shootingRange = 30f;
    public float shootingAngle = 60f;
    public Vector3 targetLandingSpot;
    public GameObject arrowProjector;

    //Slip variables.
    public bool isSlipping = false;
    //float thrust = 0f;

    //Spike variables
    public bool isMoving = false;
    public Vector3 lastPosition;
    public float distanceTravelled = 0f;
    public double enterTime;

    //Animator
    public Animator anim;

    // Use this for initialization
    void Start() {
        lastPosition = transform.position;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        //if (!isLocalPlayer || isServer)
        //    return;
        //Interact with RubberTree
        if (isThrown) {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, 1f)) {
                if (hitInfo.collider.tag == "RubberTree") {
                    GameObject rubberTreeparent = hitInfo.collider.gameObject.GetComponent<MyParent>().parent;
                    //targetLandingSpot = transform.position + (transform.forward * shootingRange);//Based on Players orientation.
                    targetLandingSpot = rubberTreeparent.transform.position + (-rubberTreeparent.transform.forward * shootingRange);//Based on Trees orientation.
                    targetLandingSpot.y = 1f;
                    Debug.DrawLine(transform.position, targetLandingSpot, Color.yellow, 10f);
                    thrust = Mathf.Sqrt((shootingRange * Physics.gravity.magnitude) / Mathf.Sin(2f * shootingAngle * Mathf.Deg2Rad));//Determine the launch velocity.
                    print("THRUST! = " + thrust);
                    thrust = Mathf.Clamp(thrust, 0f, 10f);//We need to clamp the thrust between 0f and 10f, otherwise we will get INSANE LAUNCH VELOCITY because of the angle.
                    print("thrust.... = " + thrust);
                    PlayerThrown(thrust, targetLandingSpot, shootingAngle);
                }
            }
        }
    }

    void FixedUpdate() {
        if (isThrown) {
            if (!float.IsNaN(BallisticVel(targetLandingSpot, shootingAngle).x) && !float.IsNaN(BallisticVel(targetLandingSpot, shootingAngle).y) && !float.IsNaN(BallisticVel(targetLandingSpot, shootingAngle).z)) {
                transform.GetComponent<Rigidbody>().velocity = BallisticVel(targetLandingSpot, shootingAngle);
            }
            else
                print("Nope.....");
            isThrown = false;
        }
    }

    /// <summary>
    /// Creates a balistic velocity for the player.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public Vector3 BallisticVel(Vector3 target, float angle) {
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
    public void PlayerThrown(float thrust, Vector3 landingSpot, float angle) {
        if (!isLocalPlayer || isServer)
            return;
        this.thrust = thrust;
        this.targetLandingSpot = landingSpot;
        this.shootingAngle = angle;
        isThrown = true;
        anim.SetBool("IsFlying", true);
        //Incapacitate(1);
    }

    /// <summary>
    /// Player slipped
    /// </summary>
    /// <param name="effectDuration"></param>
    /// <param name="thrust"></param>
    public void PlayerSlipped(int effectDuration, float thrust) {
        if (!isLocalPlayer || isServer)
            return;
        if (!isSlipping) {
            this.thrust = thrust;
            StartCoroutine(Slipping(effectDuration, thrust));
        }
    }

    IEnumerator Slipping(int effectDuration, float thrust) {
        isSlipping = true;
        transform.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * thrust, ForceMode.Impulse);
        CmdStun(effectDuration);
        while (effectDuration > 0) {
            yield return new WaitForSeconds(1f);
            effectDuration--;
        }
        isSlipping = false;
    }

    /// <summary>
    /// The player will take damage while moving inside a Spike Trap.
    /// </summary>
    /// <param name="player"></param>
    public void TakeDamageWhileMoving(float damage) {
        distanceTravelled = Vector3.Distance(transform.position, lastPosition);
        if (distanceTravelled >= 0.1f) {
            lastPosition = transform.position;
            print("Moved!");
            GetComponent<PlayerStats>().TakeDmg(GetComponent<PlayerStats>().maxHealth * (damage + ((float)(Network.time - enterTime) / 1000)));
        }
    }

    /// <summary>
    /// Checks whether or not the player has moved from their origin point using a threshold value.
    /// </summary>
    /// <param name="here"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    bool HasMoved(Vector3 here, float threshold) {
        bool ret = false;
        if ((lastPosition.x + threshold >= here.x &&
            lastPosition.x - threshold <= here.x) &&
            (lastPosition.z + threshold >= here.z &&
            lastPosition.z - threshold <= here.z))
            ret = true;
        return ret;
    }

    [Command]
    void CmdStun(int effectDuration) {
        transform.GetComponent<PlayerStats>().Stun(effectDuration);
    }
}
