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
    public GameObject prefab;// = Resources.Load("Prefabs/Bullet") as GameObject;
    public float impactRadius = 20;
    int healTics = 0;
    [Range(0, 1)]
    public float heal = .05f;
    public string tooltip = "Heal Force: Brings forth the soothing effects of good berries on you and your teammates, through healing. (Requires Good Berries)";
    public override string tooltipText { get { return tooltip; } }

    public override double Trigger() {
        //Play HealForce Animation
        GetComponent<NetworkAnimator>().SetTrigger("CastHealForce");
        healTics = GetComponent<SyncInventory>().HealForceBerryConsume();
        if(healTics >= 1) {
            CmdSpawnHeal();
            Vector3 PointOfImpact = transform.position + (transform.localScale.y + .5f) * transform.up;
            Collider[] hitColliders = Physics.OverlapSphere(PointOfImpact, impactRadius);
            foreach (Collider c in hitColliders) {
                if (c.tag != "Player") continue;
                if (c.GetComponentInParent<PlayerStats>().team == team) {
                    CmdHealPlayer(c.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * heal);
                    CmdHealPlayerOverTime(c.gameObject, gameObject.GetComponent<PlayerStats>().maxHealth * heal * (healTics - 1), healTics - 1);
                    // Set PlayerState to HitByHealForce
                    CmdHitPlayerAnimation(c.gameObject, PlayerBehaviour.PlayerState.HitByHealForce);
                }
            }
            return base.Trigger();
        } return 0;
    }

    public override void ShowAreaOfEffect(bool draw) {
        if (draw) {
            if (!areaOfEffect) {
                areaOfEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                areaOfEffect.GetComponent<MeshRenderer>().material = projection;
                areaOfEffect.GetComponent<Collider>().isTrigger = true;
                areaOfEffect.transform.localScale = new Vector3(impactRadius * 2, .5f, impactRadius * 2);
            }
            areaOfEffect.transform.position = transform.position + .5f * transform.up;
        } else { base.ShowAreaOfEffect(draw); }
    }

    [Command]
    void CmdSpawnHeal() {
        // Initiate GameObject using prefab, position and a rotation
        GameObject bullet = (GameObject)Instantiate(prefab, transform.position + (transform.localScale.y - .5f) * transform.up, Quaternion.identity);

        Destroy(bullet, 50); // Add whatever number fits the time of the thing
        // Spawn GameObject on Server
        NetworkServer.Spawn(bullet);
    }
}
