using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Defender
/// CrowdControlling AreaOfEffect ability
/// Friendly Fire Disabled
/// 
/// Smash (deals 1% of enemy health and stuns 1 sec) - no CD, should take 1sec to fully cast anyway
/// </summary>
public class Smash : Ability {
    public float distance;
    public float impactRadius;
    public float stunDuration = 1.0f;
    [Range(0, 1)]
    public float damage = .01f;
    public string tooltip = "Smash: Many tiny hits to shake the ground underneath your enemies and disorient them.";
    public override string tooltipText { get { return tooltip; } }

    public override double Trigger() {
        //Play Smash Animation
        GetComponent<NetworkAnimator>().SetTrigger("CastSmash");
        Vector3 PointOfImpact = transform.position + (transform.forward * distance) + (transform.localScale.y + .5f) * transform.up;
        Collider[] hitColliders = Physics.OverlapSphere(PointOfImpact, impactRadius);
        Attack(hitColliders);
        return base.Trigger();
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

    void Attack(Collider[] hitColliders) {
        foreach (Collider c in hitColliders) {
            if (c.tag != "Player" || c.gameObject == gameObject) continue;
            if (c.GetComponentInParent<PlayerStats>().team != team) {
                // Set PlayerState to HitBySmash
                CmdHitPlayerAnimation(c.gameObject, PlayerBehaviour.PlayerState.HitBySmash);//c.transform.GetComponentInParent<PlayerBehaviour>().state = PlayerBehaviour.PlayerState.HitBySmash;
                // Stun
                CmdStunPlayer(c.gameObject, stunDuration);
                //Damage
                CmdDamagePlayer(c.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * damage);
            }
        }
    }
}
