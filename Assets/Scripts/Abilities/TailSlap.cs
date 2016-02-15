using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TailSlap : Ability {
    public float distance;
    public float impactRadius;
    [Range(0, 1)]
    public float damage = .02f;
    
    public override double Trigger() {
        Vector3 PointOfImpact = transform.position + (transform.forward * distance);
        Collider[] hitColliders = Physics.OverlapSphere(PointOfImpact, impactRadius);
        if (hitColliders.Length > 0)
            print(hitColliders[0].name);
        CmdDamagePlayer(hitColliders[0].gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * damage);
        //hitColliders[0].GetComponent<PlayerStats>().CmdTakeDmg(gameObject.GetComponent<PlayerStats>().maxHealth * damage);
        return castTime;
    }
}