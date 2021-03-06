﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PunchDance : Ability {
    public float distance;
    public float impactRadius;
    public float stunDuration = 2.0f;
    public float timeBetweenAttacks = 0.1f;
    [Range(0, 1)]
    public float firstDmg = .03f;
    [Range(0, 1)]
    public float secondDmg = .04f;
    [Range(0, 1)]
    public float thirdDmg = .05f;
    public string tooltip = "Punch: A complicated maneuver that stuns and hurts the enemies.";
    public override string tooltipText { get { return tooltip; } }

    public override double Trigger() {
        //Play Punch Animation
        GetComponent<NetworkAnimator>().SetTrigger("CastPunch");
        Vector3 PointOfImpact = transform.position + (transform.forward * distance) + (transform.localScale.y + .5f) * transform.up;
        Collider[] hitColliders = Physics.OverlapSphere(PointOfImpact, impactRadius);
        //try { StartCoroutine(GetComponentInChildren<SlashEffect>().PunchDance()); } catch { }
        StartCoroutine(Attack(hitColliders));
        return base.Trigger();
    }

    IEnumerator Attack(Collider[] hitColliders) {
        foreach(Collider c in hitColliders) {
            if (c.tag != "Player" || c.gameObject == gameObject) continue;
            // Push
            CmdPushPlayer(c.gameObject, (transform.up * 5) + (transform.forward * distance));
            if (c.GetComponentInParent<PlayerStats>().team != team) {
                // Set PlayerState to HitByPunch
                CmdHitPlayerAnimation(c.gameObject, PlayerBehaviour.PlayerState.HitByPunch);
                // Stun
                CmdStunPlayer(c.gameObject, stunDuration);
                //Damage
                CmdDamagePlayer(c.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * firstDmg);
                yield return new WaitForSeconds(timeBetweenAttacks);
                CmdDamagePlayer(c.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * secondDmg);
                yield return new WaitForSeconds(timeBetweenAttacks);
                CmdDamagePlayer(c.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * thirdDmg);
            }
        }
    }

    public override void ShowAreaOfEffect(bool draw) {
        if (draw) {
            if (!areaOfEffect) {
                areaOfEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                areaOfEffect.GetComponent<MeshRenderer>().material = projection;
                areaOfEffect.GetComponent<Collider>().isTrigger = true;
                areaOfEffect.transform.localScale = new Vector3(impactRadius * 2, impactRadius * 2, impactRadius * 2);
            }
            areaOfEffect.transform.position = transform.position + (transform.forward * distance) + (transform.localScale.y + .5f) * transform.up;
        } else { base.ShowAreaOfEffect(draw); }
    }
}