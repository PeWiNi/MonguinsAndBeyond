﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Defender
/// CrowdControlling AreaOfEffect ability
/// Friendly Fire Disabled
/// 
/// Taunt (Roar/Growl/WTV) - taunts enemies (locks their target on him for 1.5 sec) CD:6 sec @15/04/16 Claus: mayhaps 'stina said something about damage..
/// 21-04-16: .08f dmg, Force-lookAt 1.5f, 2 radius around you, 
/// </summary>
public class Taunt : Ability {
    public float duration = 1.5f;
    public float radius = 2;
    [Range(0, 1)]
    public float damage = .08f;

    public override double Trigger() {
        //Play Taunt Animation
        GetComponent<NetworkAnimator>().SetTrigger("CastTaunt");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + (transform.localScale.y + .5f) * transform.up, radius);
        TauntTargets(hitColliders);
        return base.Trigger();
    }

    void TauntTargets(Collider[] hitColliders) {
        foreach (Collider c in hitColliders) {
            if (c.tag != "Player" || c.gameObject == gameObject) continue;
            if (c.GetComponentInParent<PlayerStats>().team != team) {
                // Set PlayerState to HitByTaunt
                CmdHitPlayerAnimation(c.gameObject, PlayerBehaviour.PlayerState.HitByTaunt);
                // Taunt
                CmdTauntPlayer(c.gameObject, duration);
                //Damage
                CmdDamagePlayer(c.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * damage);
                //(Maybe)TODO: <-- Insert affected by taunt animation here (unless we do a check for this in another script).
            }
        }
    }

    [Command]
    void CmdTauntPlayer(GameObject player, float duration) {
        // TODO: Implement in PlayerStats
        player.GetComponent<PlayerStats>().Taunt(gameObject, duration);
    }
}
