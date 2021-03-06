﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// 
/// 
/// Throw poison - ranged ability, slows the enemy at 0,5*speed and deals 0.5% damage*max health over 3 sec (1.5% in total). Range from 5 to 30 distance units. No CD; takes 1 sec to cast and requires poisonous herbs @12/04/16 'stina: no slow 1.5% dmg whatever 3 times @13/04/16 and just go up to 30
/// </summary>
public class PoisonDart : RotateMe {
    Vector3 Destination;
    float tickDamage = 0;
    Transform owner;
    int ticks = 3;
    float speed = 0.25f;

    Transform target;
    float maxDistance;
    Vector3 Origin;

    void FixedUpdate() {
        if (!isServer)
            return;
        if(!target) { // Original movement of dart projectile
            if (CloseEnough(transform.position, Destination, .0025f)) {
                Destroy(gameObject);
            }
            transform.position = Vector3.MoveTowards(transform.position, Destination, speed);
        } else { // Updated movement of dart projectile - chasing players
            if(Vector3.Distance(Origin, target.position) > maxDistance) {
                Destroy(gameObject);
            }
            transform.position = Vector3.MoveTowards(transform.position, target.position + (target.localScale.y + .5f) * target.up, speed);
        }
    }

    public void setup(PlayerStats ps, float damageTick, Vector3 endPos) {
        owner = ps.transform;
        tickDamage = damageTick * ps.maxHealth;
        Destination = endPos;
        speed *= (1 + (ps.Agility / 100));
    }

    public void setup(PlayerStats ps, float damageTick, Transform target, float distance) {
        owner = ps.transform;
        tickDamage = damageTick * ps.maxHealth;
        speed *= (1 + (ps.Agility / 100));
        this.target = target;
        maxDistance = distance;
        Origin = owner.transform.position;
        print(target + ", maxDist = " + distance);
    }

    void OnCollisionEnter(Collision _collision) {
        PlayerStats targetPS = _collision.transform.GetComponentInParent<PlayerStats>();
        if (targetPS != null) {
            if (targetPS.team != owner.GetComponent<PlayerStats>().team) {
                DamagePlayerOverTime(targetPS.gameObject, tickDamage, ticks);
            }
        }
        if (_collision.collider.tag != "Ability") {
            Destroy(gameObject);
        }
        Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
    }
    
    internal void DamagePlayerOverTime(GameObject player, float damageTick, float duration) {
        player.GetComponent<PlayerStats>().BadBerry(damageTick, duration, owner);
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
