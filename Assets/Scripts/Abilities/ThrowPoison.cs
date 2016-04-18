using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Supporter
/// Damaging SingleTarget Projectile ability
/// 
/// Throw poison - ranged ability, slows the enemy at 0,5*speed and deals 0.5% damage*max health over 3 sec (1.5% in total). Range from 5 to 30 distance units. No CD; takes 1 sec to cast and requires poisonous herbs @12/04/16 'stina: no slow 1.5% dmg whatever 3 times
/// Ties with SyncInventory.ThrowPoisonBerryConsume()
/// This script is tied to the Scripts/Abilities/PoisonDart.cs, defining how the Dart should behave when Triggered()
/// </summary>
public class ThrowPoison : Ability {
    public GameObject prefab;// = Resources.Load("Prefabs/Bullet") as GameObject;
    public float distance = 30f;
    [Range(0, 1)]
    public float damage = 0.015f;

    public override double Trigger() {
        if (!GetComponent<SyncInventory>().ThrowPoisonBerryConsume())
            return 0;
        if(GetComponent<Aim>().aiming) { Cancel(); return 0; }
        StartCoroutine(GetComponent<Aim>().Poisony(this));
        return 0;
    }

    public void Throw(Vector3 pos) {
        //Play ThrowPoison Animation
        GetComponent<NetworkAnimator>().SetTrigger("CastThrowPoison");
        CmdDoFire(pos);
        base.Trigger();
        timer = (float)Network.time;
    }

    public void Cancel() {
        GetComponent<SyncInventory>().pickupBerry(Herb.Condition.Degenration);
    }

    [Command]
    void CmdDoFire(Vector3 endPos) {
        // Initiate GameObject using prefab, position and a rotation
        GameObject bullet = (GameObject)Instantiate( // Offset by 5?
            prefab, transform.position + (transform.localScale.x + .5f) * transform.forward + (transform.localScale.y + .5f) * transform.up,
            Quaternion.identity);
        // Determine end-position of Projectile
        Vector3 pos = endPos == new Vector3() ? (transform.position + (transform.forward * distance)) : endPos;
        // Pass correct parameters from the Player Prefab
        bullet.GetComponent<PoisonDart>().setup(GetComponent<PlayerStats>(), damage, pos);

        // Spawn GameObject on Server
        NetworkServer.Spawn(bullet);
    }
}
