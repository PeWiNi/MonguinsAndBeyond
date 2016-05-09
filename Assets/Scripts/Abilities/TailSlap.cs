using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TailSlap : Ability {
    public float distance;
    public float impactRadius;
    [Range(0, 1)]
    public float damage = .02f;
    public string tooltip = "Tail Slap: One quick hit, for a bit of damage.";
    public override string tooltipText { get { return tooltip; } }

    public override double Trigger() {
        //Play TailSlap Animation
        GetComponent<NetworkAnimator>().SetTrigger("CastTailSlap");
        Vector3 PointOfImpact = transform.position + (transform.forward * distance) + (transform.localScale.y + .5f) * transform.up;
        Collider[] hitColliders = Physics.OverlapSphere(PointOfImpact, impactRadius);
        //try { StartCoroutine(GetComponentInChildren<SlashEffect>().TailSlap()); } catch { }
        foreach(Collider hit in hitColliders) {
            if(hit.tag == "Player")
                if (hit.GetComponentInParent<PlayerStats>().team != team) {
                    CmdDamagePlayer(hit.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * damage);
                    // Set PlayerState to HitByTailSlap
                    CmdHitPlayerAnimation(hit.gameObject, PlayerBehaviour.PlayerState.HitByTailSlap);
                    //hit.transform.GetComponentInParent<PlayerBehaviour>().state = PlayerBehaviour.PlayerState.HitByTailSlap;
                    break;
                }
        }
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
}