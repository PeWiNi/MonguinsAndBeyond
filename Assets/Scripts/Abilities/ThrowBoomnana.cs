using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

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
        return castTime;
    }

    [Command]
    void CmdDoFire() {
        GameObject bullet = (GameObject)Instantiate(
            prefab, transform.position + (transform.localScale.x * transform.forward),
            Quaternion.identity);

        bullet.GetComponent<Boomnana>().setup(gameObject, distance, speed, fullDamage, selfDamage);

        NetworkServer.Spawn(bullet);
    }
}
