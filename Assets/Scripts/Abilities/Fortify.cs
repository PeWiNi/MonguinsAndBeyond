using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
/// <summary>
/// Defender 
/// Enhancement ability
/// 
/// Fortify - temporarily increase health and resilience of the defender with 20% for 5 sec. CD:20sec
/// 21-04-16: reduce damage taken (temp resi boost), duration = 10
/// </summary>
public class Fortify : Ability {
    public float duration = 10;
    public float effect = 20;
    public string tooltip = "Fortify: Increases the durability of skin for 20 seconds - acts as resilience boost.";
    public override string tooltipText { get { return tooltip; } }

    public override double Trigger() {
        //Play Fortify Animation
        GetComponent<Animator>().SetTrigger("CastFortify");
        CmdFortify(gameObject, duration);
        return base.Trigger();
    }

    public override void ShowAreaOfEffect(bool draw) {
        if (draw) {
            if(!areaOfEffect) {
                areaOfEffect = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                areaOfEffect.GetComponent<MeshRenderer>().material = projection;
                areaOfEffect.GetComponent<Collider>().isTrigger = true;
                areaOfEffect.transform.localScale = transform.localScale;
            }
            areaOfEffect.transform.position = transform.position + transform.localScale.y * Vector3.up;
        } else { base.ShowAreaOfEffect(draw); }
    }

    [Command]
    void CmdFortify(GameObject player, float duration) {
        // TODO: Implement in PlayerStats
        player.GetComponent<PlayerStats>().Fortify(effect / 100, duration);
    }
}
