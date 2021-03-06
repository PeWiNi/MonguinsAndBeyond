﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Attacker
/// Damaging Projectile AreaOfEffect ability
/// Friendly Fire Enabled
/// 
/// A boomerang attack that is trown forward for a specified range, and returns towards the user if it does not hit anything
/// Damages user upon return
/// Enemies takes full damage, while friends takes less
/// 
/// This script is tied to the Scripts/Abilities/Boomnana.cs, defining how the BOOMnana should behave when Triggered()
/// </summary>
public class ThrowBoomnana : Ability
{
    public GameObject prefab; //= Resources.Load("Prefabs/Bullet") as GameObject;
    public float distance = 12f;
    public float speed = 2f;
    [Range(0, 1)]
    public float fullDamage = 0.8f;
    [Range(0, 1)]
    public float selfDamage = 0.35f;
    public string tooltip = "BOOMnana: A really skillful action that creates explosions on impact.";
    public override string tooltipText { get { return tooltip; } }
    [SyncVar]
    Transform target;

    public override double Trigger() {
        CmdTarget(gameObject);
        StartCoroutine(GetComponent<Aim>().Boomy(this));
        GetComponent<NetworkAnimator>().SetTrigger("Aim");
        //CmdDoFire(new Vector3());
        return 0;
    }

    public override void ShowAreaOfEffect(bool draw) {
        if (draw) {
            if (!areaOfEffect) {
                areaOfEffect = GameObject.CreatePrimitive(PrimitiveType.Cube);
                areaOfEffect.GetComponent<MeshRenderer>().material = projection;
                areaOfEffect.GetComponent<Collider>().isTrigger = true;
                areaOfEffect.transform.localScale = new Vector3(1, 1, distance);
                areaOfEffect.transform.rotation = transform.rotation;
            }
            areaOfEffect.transform.position = transform.position + Vector3.up * .5f + (distance / 2) * transform.forward;
        } else { base.ShowAreaOfEffect(draw); }
    }

    public void Throw(Vector3 pos, Transform target) {
        CmdTarget(target.gameObject);
        GetComponent<NetworkAnimator>().SetTrigger("CastBOOMnana");
        CmdDoFire(pos);
        base.Trigger();
        print(target + ", maxDist = " + distance);
    }

    public void Cancel() {
        GetComponent<NetworkAnimator>().SetTrigger("CancelAim");
    }
    [Command]
    void CmdTarget(GameObject targetGO) { target = targetGO.transform; }

    [Command]
    void CmdDoFire(Vector3 endPos) {
        // Initiate GameObject using prefab, position and a rotation
        GameObject bullet = (GameObject)Instantiate(
            prefab, transform.position + (transform.localScale.x + .5f) * transform.forward + (transform.localScale.y + .5f) * transform.up,
            Quaternion.identity);
        if(target == transform) {
            // Determine end-position of BOOMnana
            Vector3 pos = endPos == new Vector3() ? (transform.position + (transform.forward * distance)) : endPos;
            // Pass correct parameters from the Player Prefab
            bullet.GetComponent<Boomnana>().setup(gameObject, pos, speed, fullDamage, selfDamage);
        } else {
            // Pass correct parameters from the Player Prefab
            bullet.GetComponent<Boomnana>().setup(gameObject, target, distance, speed, fullDamage, selfDamage);
        }

        // Spawn GameObject on Server
        NetworkServer.Spawn(bullet);
    }

    public Aim Aim {
        get {
            throw new System.NotImplementedException();
        }

        set {
        }
    }
}
