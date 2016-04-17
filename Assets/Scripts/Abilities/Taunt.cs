using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
/// <summary>
/// Defender
/// CrowdControlling AreaOfEffect ability
/// Friendly Fire Disabled
/// 
/// Taunt (Roar/Growl/WTV) - taunts enemies (locks their target on him for 3 sec) CD:6 sec @15/04/16 Claus: mayhaps 'stina said something about damage..
/// </summary>
public class Taunt : Ability {
    public float duration = 3;
    public float range;
    //[Range(0, 1)]
    //public float damage = .01f;

    public override double Trigger() {
        //Play Taunt Animation
        GetComponent<Animator>().SetTrigger("CastTaunt");
        GetComponent<Animator>().SetLayerWeight(1, 1f);
        Collider[] hitColliders = Physics.OverlapSphere((transform.localScale.y + .5f) * transform.up, range);
        TauntTargets(hitColliders);
        return base.Trigger();
    }

    void TauntTargets(Collider[] hitColliders) {
        foreach (Collider c in hitColliders) {
            if (c.tag != "Player" || c.gameObject == gameObject) continue;
            if (c.GetComponentInParent<PlayerStats>().team != team) {
                // Stun
                CmdTauntPlayer(c.gameObject, duration);
                //Damage
                //CmdDamagePlayer(c.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * damage);
            }
        }
    }

    [Command]
    void CmdTauntPlayer(GameObject player, float duration) {
        // TODO: Implement in PlayerStats
        player.GetComponent<PlayerStats>().Taunt(gameObject, duration);
    }
}
