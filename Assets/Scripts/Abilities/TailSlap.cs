using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TailSlap : Ability {
    public float distance;
    public float impactRadius;
    [Range(0, 1)]
    public float damage = .02f;
    
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
                    hit.transform.GetComponentInParent<PlayerBehaviour>().state = PlayerBehaviour.PlayerState.HitByTailSlap;
                    break;
                }
        }
        return base.Trigger();
    }
}