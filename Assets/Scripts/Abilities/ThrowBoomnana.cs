using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Attacker
/// Damaging Projectile AreaOfEffect ability
/// Friendly Fire Enabled
/// 
/// A boomerang attack that is trown forward for a specified range, and returns towards the user if it does not hit anything
/// Damages user upon return
/// Enemies takes full damage, while friends takes less
/// 
/// This script is tied to the Scripts/Abilities/Boomnana.cs, defining how the BOOMnana should behave when Triggered()
/// </summary>
public class ThrowBoomnana : Ability
{
    public GameObject prefab; //= Resources.Load("Prefabs/Bullet") as GameObject;
    public float distance = 12f;
    public float speed = 2f;
    [Range(0, 1)]
    public float fullDamage = 0.8f;
    [Range(0, 1)]
    public float selfDamage = 0.35f;
    public string tooltip = "BOOMnana: A really skillful action that creates explosions on impact.";
    public override string tooltipText { get { return tooltip; } }

    public override double Trigger() {
        StartCoroutine(GetComponent<Aim>().Boomy(this));
        GetComponent<NetworkAnimator>().SetTrigger("Aim");
        //CmdDoFire(new Vector3());
        return 0;
    }

    public void Throw(Vector3 pos) {
        GetComponent<NetworkAnimator>().SetTrigger("CastBOOMnana");
        CmdDoFire(pos);
        timer = (float)Network.time;
    }

    public void Cancel() {
        GetComponent<NetworkAnimator>().SetTrigger("CancelAim");
    }

    [Command]
    void CmdDoFire(Vector3 endPos) {
        // Initiate GameObject using prefab, position and a rotation
        GameObject bullet = (GameObject)Instantiate(
            prefab, transform.position + (transform.localScale.x + .5f) * transform.forward + (transform.localScale.y + .5f) * transform.up,
            Quaternion.identity);
        // Determine end-position of BOOMnana
        Vector3 pos = endPos == new Vector3() ? (transform.position + (transform.forward * distance)) : endPos;
        // Pass correct parameters from the Player Prefab
        bullet.GetComponent<Boomnana>().setup(gameObject, pos, speed, fullDamage, selfDamage);

        // Spawn GameObject on Server
        NetworkServer.Spawn(bullet);
    }
}
