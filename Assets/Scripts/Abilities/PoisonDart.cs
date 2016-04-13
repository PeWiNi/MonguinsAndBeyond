using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// 
/// 
/// Throw poison - ranged ability, slows the enemy at 0,5*speed and deals 0.5% damage*max health over 3 sec (1.5% in total). Range from 5 to 30 distance units. No CD; takes 1 sec to cast and requires poisonous herbs @12/04/16 'stina: no slow 1.5% dmg whatever 3 times @13/04/16 and just go up to 30
/// </summary>
public class PoisonDart : NetworkBehaviour {
    Vector3 Destination;
    float tickDamage = 0;
    int ownerTeam;
    float speed = 0.25f;

    void FixedUpdate() {
        if (!isServer)
            return;
        if (CloseEnough(transform.position, Destination, .0025f)) {
            Destroy(gameObject);
        }
        transform.position = Vector3.MoveTowards(transform.position, Destination, speed);
    }

    public void setup(PlayerStats ps, float damageTick, Vector3 endPos) {
        ownerTeam = ps.team;
        tickDamage = damageTick * ps.maxHealth;
        Destination = endPos;
        speed *= (1 + (ps.agility / 100));
    }

    void OnCollisionEnter(Collision _collision) {
        PlayerStats targetPS = _collision.transform.GetComponentInParent<PlayerStats>();
        if (targetPS != null) {
            if (targetPS.team != ownerTeam) {
                DamagePlayerOverTime(targetPS.gameObject, tickDamage, 3);
            }
        }
        if (_collision.collider.tag != "Ability") {
            Destroy(gameObject);
        }
        Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
    }
    
    internal void DamagePlayerOverTime(GameObject player, float damageTick, float duration) {
        player.GetComponent<PlayerStats>().BadBerry(damageTick, duration);
    }
    internal void SlowPlayer(GameObject player, float duration) {
        player.GetComponent<PlayerStats>().Slow(true, duration);
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
        if ((transform.position.x + threshold >= there.x &&
            transform.position.x - threshold <= there.x) &&
            (transform.position.z + threshold >= there.z &&
            transform.position.z - threshold <= there.z))
            ret = true;
        return ret;
    }
}
