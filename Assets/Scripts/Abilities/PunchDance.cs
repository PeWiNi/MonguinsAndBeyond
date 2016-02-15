using UnityEngine;
using System.Collections;

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

    public override double Trigger() {
        StartCoroutine("Attack");
        return castTime;
    }

    IEnumerator Attack() {
        Vector3 PointOfImpact = transform.position + (transform.forward * distance);
        Collider[] hitColliders = Physics.OverlapSphere(PointOfImpact, impactRadius);
        if(hitColliders.Length > 0) {
            CmdStunPlayer(hitColliders[0].gameObject, stunDuration);
            CmdDamagePlayer(hitColliders[0].gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * firstDmg);
            yield return new WaitForSeconds(timeBetweenAttacks);
            CmdDamagePlayer(hitColliders[0].gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * secondDmg);
            yield return new WaitForSeconds(timeBetweenAttacks);
            CmdDamagePlayer(hitColliders[0].gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * thirdDmg);
        }
    }
}