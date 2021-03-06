﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Supporter
/// Damaging SingleTarget Projectile ability
/// 
/// Throw poison - ranged ability, slows the enemy at 0,5*speed and deals 0.5% damage*max health over 3 sec (1.5% in total). Range from 5 to 30 distance units. No CD; takes 1 sec to cast and requires poisonous herbs @12/04/16 'stina: no slow 1.5% dmg whatever 3 times
/// Ties with SyncInventory.ThrowPoisonBerryConsume()
/// This script is tied to the Scripts/Abilities/PoisonDart.cs, defining how the Dart should behave when Triggered()
/// </summary>
public class ThrowPoison : Ability {
    public GameObject prefab;// = Resources.Load("Prefabs/Bullet") as GameObject;
    public float distance = 30f;
    [Range(0, 1)]
    public float damage = 0.015f;
    public string tooltip = "Poison Dart: Throws a poisonous berry at your enemies, assuring they are affected by the its effect. (Requires Bad Berries)";
    public override string tooltipText { get { return tooltip; } }
    [SyncVar]
    Transform target;

    public override double Trigger() {
        CmdTarget(gameObject);
        if (!GetComponent<SyncInventory>().ThrowPoisonBerryConsume())
            return 0;
        if(GetComponent<Aim>().aiming) { Cancel(); return 0; }
        StartCoroutine(GetComponent<Aim>().Poisony(this));
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
        //Play ThrowPoison Animation
        GetComponent<NetworkAnimator>().SetTrigger("CastThrowPoison");
        CmdDoFire(pos);
        base.Trigger();
        print(target + ", maxDist = " + distance);
    }

    public void Cancel() {
        GetComponent<SyncInventory>().pickupBerry(Herb.Condition.Degenration);
    }
    [Command]
    void CmdTarget(GameObject targetGO) { target = targetGO.transform; }

    [Command]
    void CmdDoFire(Vector3 endPos) {
        // Initiate GameObject using prefab, position and a rotation
        GameObject bullet = (GameObject)Instantiate( // Offset by 5?
            prefab, transform.position + (transform.localScale.x + .5f) * transform.forward + (transform.localScale.y + .5f) * transform.up,
            Quaternion.identity);
        if(target == transform) {
            // Determine end-position of Projectile
            Vector3 pos = endPos == new Vector3() ? (transform.position + (transform.forward * distance)) : endPos;
            // Pass correct parameters from the Player Prefab
            bullet.GetComponent<PoisonDart>().setup(GetComponent<PlayerStats>(), damage, pos);
        } else {
            bullet.GetComponent<PoisonDart>().setup(GetComponent<PlayerStats>(), damage, target, distance);
            bullet.transform.LookAt(target.position);
            bullet.GetComponent<PoisonDart>().SetRotation(bullet.transform.rotation);
        }

        // Spawn GameObject on Server
        NetworkServer.Spawn(bullet);
    }

    public override bool OnCooldown() {
        if (GetComponent<SyncInventory>().GetCount("BerryB") == 0)
            return true;
        return base.OnCooldown();
    }

    public override float CooldownRemaining() {
        if (GetComponent<SyncInventory>().GetCount("BerryB") == 0)
            return -1;
        return base.CooldownRemaining();
    }

    public Aim Aim {
        get {
            throw new System.NotImplementedException();
        }

        set {
        }
    }
}
