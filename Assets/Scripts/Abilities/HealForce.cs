using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Supporter
/// Healing AreaOfEffect ability
/// 
/// Heal force - ability targets only friendly characters. Heals 50-250 HP over 3 sec depending on skill and herbs used in the ability. Max range 20 units. 1 herb heals instantly for 50HP, 2->4 herb heal over time (50 at first and 50 more for each 'tic'). No CD; instant application; requires herbs to cast
/// Ties with SyncInventory.HealForceBerryConsume()
/// </summary>
public class HealForce : Ability {
    public float impactRadius = 20;
    int healTics = 0;
    [Range(0, 1)]
    public float heal = .05f;

    public override double Trigger() {
        //Play HealForce Animation
        GetComponent<NetworkAnimator>().SetTrigger("CastHealForce");
        healTics = GetComponent<SyncInventory>().HealForceBerryConsume();
        if(healTics >= 1) {
            Vector3 PointOfImpact = transform.position + (transform.localScale.y + .5f) * transform.up;
            Collider[] hitColliders = Physics.OverlapSphere(PointOfImpact, impactRadius);
            foreach (Collider c in hitColliders) {
                if (c.tag != "Player") continue;
                if (c.GetComponentInParent<PlayerStats>().team == team) {
                    CmdHealPlayer(c.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * heal);
                    CmdHealPlayerOverTime(c.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * heal, healTics - 1);
                    c.transform.GetComponentInParent<PlayerBehaviour>().state = PlayerBehaviour.PlayerState.HitByHealForce;
                }
            }
            return base.Trigger();
        } return 0;
    }
}
