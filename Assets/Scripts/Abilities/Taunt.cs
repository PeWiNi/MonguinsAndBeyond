using UnityEngine;
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
    public string tooltip = "Taunt: Forces your enemies to look at you and only you for a brief moment.";
    public override string tooltipText { get { return tooltip; } }

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

    public override void ShowAreaOfEffect(bool draw) {
        if (draw) {
            if (!areaOfEffect) {
                areaOfEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                areaOfEffect.GetComponent<MeshRenderer>().material = projection;
                areaOfEffect.GetComponent<Collider>().isTrigger = true;
                areaOfEffect.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
            }
            areaOfEffect.transform.position = transform.position + (transform.localScale.y + .5f) * transform.up;
        } else { base.ShowAreaOfEffect(draw); }
    }

    [Command]
    void CmdTauntPlayer(GameObject player, float duration) {
        // TODO: Implement in PlayerStats
        player.GetComponent<PlayerStats>().Taunt(gameObject, duration);
    }
}
