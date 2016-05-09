using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Supporter
/// Damaging Ranged AreaOfEffect Ability
/// 
/// Puke - (the old puke, does the same thing) stuns all enemies in range, has about 2 units distance units in range. Channeled 3 sec; CD:5 sec
/// 21-04-16: 2 area in front of you, no trigger, .08f dmg & slow (3s) 1 tick, no channeling, 10s cooldown
/// </summary>
public class Puke : Ability {
    public float area = 2;
    public float slowDuration = 3.0f;
    [Range(0, 1)]
    public float damage = .08f;
    public string tooltip = "Puke: Covers the area around you in banana and fish slime that slows and hurts your enemies.";
    public override string tooltipText { get { return tooltip; } }

    public override double Trigger() {
        //Play Puke Animation
        GetComponent<NetworkAnimator>().SetTrigger("CastPuke");
        Pukey();
        return base.Trigger();
    }

    void Pukey() {
        gameObject.GetComponent<PlayerStats>().Incapacitate((float)castTime);
            Collider[] hitColliders = Physics.OverlapSphere(transform.position + ((transform.localScale.y + .5f) * transform.up) + (transform.forward * area / 2), area / 2);
            foreach (Collider c in hitColliders) {
                if (c.tag != "Player" || c.gameObject == gameObject) continue;
                if (c.GetComponentInParent<PlayerStats>().team != team) {
                    // Set PlayerState to HitByPuke
                    CmdHitPlayerAnimation(c.gameObject, PlayerBehaviour.PlayerState.HitByPuke);
                    // Slow
                    CmdSlowPlayer(c.gameObject, slowDuration);
                    //Damage
                    CmdDamagePlayer(c.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * damage);
                }
            }
    }

    public override void ShowAreaOfEffect(bool draw) {
        if (draw) {
            if (!areaOfEffect) {
                areaOfEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                areaOfEffect.GetComponent<MeshRenderer>().material = projection;
                areaOfEffect.GetComponent<Collider>().isTrigger = true;
                areaOfEffect.transform.localScale = new Vector3(area, area, area);
            }
            areaOfEffect.transform.position = transform.position + ((transform.localScale.y + .5f) * transform.up) + (transform.forward * area / 2);
        } else { base.ShowAreaOfEffect(draw); }
    }
}
