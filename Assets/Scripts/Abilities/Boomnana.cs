using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Attacker
/// Damaging projectile ability
/// Friendly Fire Enabled
/// 
/// A boomerang attack that is trown forward for a specified range, and returns towards the user if it does not hit anything
/// Damages user upon return
/// Enemies takes full damage, while friends takes less
/// THOUGHT: Should the BOOMnana have Area Damage (with diminishing effects the further away other monguins are from impact point)
/// 
/// This script is tied to the Scripts/Abilities/ThrowBoomnana.cs
/// </summary>
public class Boomnana : NetworkBehaviour {
    float damage;
    public GameObject owner;
    float speed;
    float fullDamage;
    float selfDamage;
    int ownerTeam;
    bool movingBack = false;
    Vector3 endpoint;

    #region AoE
    public bool areaOfEffect = true;
    [Tooltip("Total size of the AoE")]
    public float maxArea = 3f;
    [Tooltip("The area inside, in which they take full damage")]
    /// <summary>
    /// Distance is based on center of both colliders, 
    /// so two 1x1x1 objects will require a innerRadius of at least 1 to trigger this effect
    /// </summary>
    public float innerRadius = 1f;
    #endregion

    // Use this for initialization
    void Start() {

    }

    /// <summary>
    /// A seperate Start function, created because the BOOMnana needs to be setup properly
    /// </summary>
    /// <param name="owner">GameObject of the player who threw the BOOMnana</param>
    /// <param name="endPos">The position to which the BOOMnana will fly before returning</param>
    /// <param name="spd">The speed at which the BOOMnana will fly</param>
    /// <param name="fullDmg">The maximum damage dealt to opponent players</param>
    /// <param name="selfDmg">Reduced damage taken if the BOOMnana is unsuccessful and returns to the user</param>
    public void setup(GameObject owner, Vector3 endPos, float spd, float fullDmg, float selfDmg) {
        this.owner = owner;
        ownerTeam = owner.GetComponent<PlayerStats>().team;
        damage = owner.GetComponent<PlayerStats>().maxHealth; //TODO: Get AGI and calculate DMG modifier -- do for all Abilities
        speed = spd;
        fullDamage = fullDmg;
        selfDamage = selfDmg;
        endpoint = doNotTouchTerrain(endPos, false);
        movingBack = false;
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!isServer)
            return;
        if (CloseEnough(transform.position, endpoint, .0025f)) {
            movingBack = true;
        }
        if (movingBack) {
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            transform.position = doNotTouchTerrain(Vector3.MoveTowards(transform.position, owner.gameObject.transform.position, speed));
        } else {
            transform.position = doNotTouchTerrain(Vector3.MoveTowards(transform.position, endpoint, speed));
        }
    }

    /// <summary>
    /// Checks if the distance between two Vector3s is less than a certain threshold value
    /// Tailored for determining if the boomnana has reached its "endpoint", so only accounts for x and z
    /// </summary>
    /// <param name="here">Current Vector3 position of the object</param>
    /// <param name="there">Desired point which "here" is supposed to reach</param>
    /// <param name="threshold">Floating point margin of error</param>
    /// <returns></returns>
    bool CloseEnough(Vector3 here, Vector3 there, float threshold) {
        bool ret = false;
        if ((transform.position.x + threshold >= endpoint.x && 
            transform.position.x - threshold <= endpoint.x) &&
            (transform.position.z + threshold >= endpoint.z &&
            transform.position.z - threshold <= endpoint.z))
            ret = true;
        return ret;
    }

    /// <summary>
    /// Keeps the y from hitting the terrain (except in extreme conditions)
    /// </summary>
    /// <param name="pos">Current position of the object</param>
    /// <param name="lerp">Whether or not the returned position should be lerped between desired and current position</param>
    /// <returns>Position away from the terrain</returns>
    Vector3 doNotTouchTerrain(Vector3 pos, bool lerp = true) {
        Vector3 hoverPos = pos;
        RaycastHit hit;
        if (Physics.Raycast(pos, -Vector3.up, out hit)) {
            var distancetoground = hit.distance;
            var heightToAdd = transform.localScale.y + .5f;
            hoverPos.y = pos.y - distancetoground + heightToAdd;
        }
        if (lerp) return Vector3.Lerp(pos, hoverPos, Time.deltaTime * 5);
        else return hoverPos;
    }

    /// <summary>
    /// Only damage self if the BOOMnana is returning and hits you
    /// Teammates takes the same damage as the player who could have hit themselves
    /// Damage the opponent with the full damage
    /// 
    /// Damage is scaled according to the owner's maxHealth (the player who threw the BOOMnana)
    /// </summary>
    /// <param name="_collision"></param>
    void OnCollisionEnter(Collision _collision) {
        bool me = _collision.gameObject == owner;
        if (areaOfEffect) {
            AoE();
        } else {
            PlayerStats targetPS = _collision.transform.GetComponentInParent<PlayerStats>();
            if (targetPS != null) {
                if ((me && movingBack) || (targetPS.team == ownerTeam && _collision.gameObject != owner)) {
                    targetPS.TakeDmg(damage * selfDamage); // Nana is spawned and is on server, trigger on client
                } else {
                    if (_collision.collider.tag == "Player" && targetPS.team != ownerTeam) {
                        targetPS.TakeDmg(damage * fullDamage); // Nana is spawned and is on server, trigger on client
                    }
                }
            }
        }
        if (_collision.collider.tag != "Ability") {
            BOOM();
            Destroy(gameObject);
        }
        Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
    }

    /// <summary>
    /// BOOMnana VFXs
    /// </summary>
    void BOOM() {
        GameObject projector = (GameObject)Instantiate(
            Resources.Load("Prefabs/BOOM_Projector") as GameObject, transform.position,
            Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));

        projector.GetComponent<Projector>().orthographicSize = maxArea * owner.GetComponent<PlayerStats>().sizeModifier;
        // THOUGHT: Maybe do a alpha dropoff script to allow the projector to fade over time (and then destroy when alpha == 0)
        Destroy(projector, 10);

        NetworkServer.Spawn(projector);
    }

    void AoE() {
        float maxDist = maxArea * owner.GetComponent<PlayerStats>().sizeModifier;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, maxArea);
        float dmg = 0;
        PlayerStats targetPS = null;
        foreach (Collider _collider in hitColliders) {
            if (_collider.tag == "Player") { // Check if player
                targetPS = _collider.transform.GetComponentInParent<PlayerStats>();
                // Determine maxDamage based on team
                dmg = damage * (targetPS.team != ownerTeam ? fullDamage : selfDamage);
                float dist = Vector3.Distance(transform.position, _collider.transform.position);
                if (dist > innerRadius)  // If inside the radius (
                    dmg *= ((maxArea - (dist)) / (maxArea));
                targetPS.TakeDmg(dmg);
            }
        }
        /*
        Debug.DrawLine(transform.position, transform.position + transform.up * maxDist, Color.yellow, 10);
        Debug.DrawLine(transform.position, transform.position + transform.right * maxDist, Color.yellow, 10);
        Debug.DrawLine(transform.position, transform.position + -transform.right * maxDist, Color.yellow, 10);
        Debug.DrawLine(transform.position, transform.position + transform.forward * maxDist, Color.yellow, 10);
        Debug.DrawLine(transform.position, transform.position + -transform.forward * maxDist, Color.yellow, 10);
        */
    }
}
