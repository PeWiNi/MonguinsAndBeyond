using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Supporter
/// Damaging Ranged AreaOfEffect Ability
/// 
/// Puke - (the old puke, does the same thing) stuns all enemies in range, has about 2 units distance units in range. Channeled 3 sec; CD:5 sec
/// </summary>
public class Puke : Ability
{
    public float distance;
    public float impactRadius;
    public float stunDuration = 0.0f;
    [Range(0, 1)]
    public float damage = .00f;
    float triggerRate = 1;

    public override double Trigger() {
        StartCoroutine(GetComponent<Aim>().Pukey(this));
        //TODO: <--Start the aim state for Puke Animation
        return base.Trigger();
    }

    public void Puking(Vector3 pos) {
        //Play Puke Animation
        GetComponent<NetworkAnimator>().SetTrigger("CastPuke");
        StartCoroutine(Attack(pos));
        timer = (float)Network.time;
    }

    public void Cancel() {
        //TODO: <--Quit aim state for Puke Animation
    }

    IEnumerator Attack(Vector3 pos) {
        double timeSpent = 0;
        gameObject.GetComponent<PlayerStats>().Incapacitate((float)castTime);
        while (timeSpent < castTime) {
            Collider[] hitColliders = Physics.OverlapSphere(pos, impactRadius);
            foreach (Collider c in hitColliders) {
                if (c.tag != "Player" || c.gameObject == gameObject) continue;
                if (c.GetComponentInParent<PlayerStats>().team != team) {
                    // Set PlayerState to HitByPuke
                    CmdHitPlayerAnimation(c.gameObject, PlayerBehaviour.PlayerState.HitByPuke);
                    // Stun
                    CmdStunPlayer(c.gameObject, stunDuration);
                    //Damage
                    CmdDamagePlayer(c.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * damage);
                }
            }
            yield return new WaitForSeconds(triggerRate);
            timeSpent += triggerRate;
        }
    }
}
