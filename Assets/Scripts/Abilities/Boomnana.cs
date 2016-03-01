﻿using UnityEngine;
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

    // Use this for initialization
    void Start() {

    }

    /// <summary>
    /// A seperate Start function, created because the BOOMnana needs to be setup properly
    /// </summary>
    /// <param name="owner">GameObject of the player who threw the BOOMnana</param>
    /// <param name="distance">The distance that the BOOMnana can fly</param>
    /// <param name="spd">The speed at which the BOOMnana will fly</param>
    /// <param name="fullDmg">The maximum damage dealt to opponent players</param>
    /// <param name="selfDmg">Reduced damage taken if the BOOMnana is unsuccessful and returns to the user</param>
    public void setup(GameObject owner, float distance, float spd, float fullDmg, float selfDmg) {
        this.owner = owner;
        ownerTeam = owner.GetComponent<PlayerStats>().team;
        damage = owner.GetComponent<PlayerStats>().maxHealth;
        speed = spd;
        fullDamage = fullDmg;
        selfDamage = selfDmg;
        endpoint = owner.transform.position + (owner.transform.forward * distance);
        movingBack = false;
    }

    // Update is called once per frame
    void Update() {
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

    bool CloseEnough(Vector3 here, Vector3 there, float threshold) {
        bool ret = false;
        if ((transform.position.x + threshold >= endpoint.x && 
            transform.position.x - threshold <= endpoint.x) &&
            (transform.position.z + threshold >= endpoint.z &&
            transform.position.z - threshold <= endpoint.z))
            ret = true;
        return ret;
    }

    Vector3 doNotTouchTerrain(Vector3 pos) {
        Vector3 hoverPos = pos;
        RaycastHit hit;
        if (Physics.Raycast(pos, -Vector3.up, out hit)) {
            var distancetoground = hit.distance;
            var heightToAdd = transform.localScale.y + .5f;
            hoverPos.y = pos.y - distancetoground + heightToAdd;
        }
        pos = Vector3.Lerp(pos, hoverPos, Time.deltaTime * 5);
        return pos;
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
        PlayerStats targetPS = _collision.transform.GetComponentInParent<PlayerStats>();
        if(targetPS != null) {
            if ((me && movingBack) || (targetPS.team == ownerTeam && _collision.gameObject != owner))
                targetPS.TakeDmg(damage * selfDamage); // Nana is spawned and is on server, trigger on client
            else {
                if (_collision.collider.tag == "Player" && targetPS.team != ownerTeam) {
                    targetPS.TakeDmg(damage * fullDamage); // Nana is spawned and is on server, trigger on client
                }
            }
        }
        if (_collision.collider.tag != "Ability")
            Destroy(gameObject);
        Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
    }
}
