using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Attacker
/// Damaging projectile ability
/// Friendly Fire Enabled
/// 
/// A boomerang attack that is trown forward for a specified range, and returns towards the user if it does not hit anything
/// Damages user upon return
/// Enemies takes full damage, while friends takes less
/// 
/// This script is tied to the Scripts/Abilities/Boomnana.cs, defining how the BOOMnana should behave when Triggered()
/// </summary>
public class ThrowBoomnana : Ability {
    public GameObject prefab; //= Resources.Load("Prefabs/Bullet") as GameObject;
    public float distance = 12f;
    public float speed = 1f;
    [Range(0, 1)]
    public float fullDamage = 0.8f;
    [Range(0, 1)]
    public float selfDamage = 0.35f;

    public override double Trigger() {
        //GetComponentInParent<PlayerStats>().CmdDoFire(3.0f);
        CmdDoFire();
        return base.Trigger();
    }

    [Command]
    void CmdDoFire() {
        // Initiate GameObject using prefab, position and a rotation
        GameObject bullet = (GameObject)Instantiate(
            prefab, transform.position + transform.localScale.x * transform.forward + GetComponent<Rigidbody>().velocity.magnitude * transform.forward,
            Quaternion.identity);

        // Pass correct parameters from the Player Prefab
        bullet.GetComponent<Boomnana>().setup(gameObject, distance, speed, fullDamage, selfDamage);

        // Spawn GameObject on Server
        NetworkServer.Spawn(bullet);
    }
}
