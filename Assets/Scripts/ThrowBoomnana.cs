using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ThrowBoomnana : Ability {
    public GameObject bulletPrefab; //= Resources.Load("Prefabs/Bullet") as GameObject;

    public override void Trigger() {
        //GetComponentInParent<PlayerStats>().CmdDoFire(3.0f);
        CmdDoFire();
    }

    [Command]
    void CmdDoFire() {
        GameObject bullet = (GameObject)Instantiate(
            bulletPrefab, transform.position + (transform.localScale.x * transform.forward),
            Quaternion.identity);

        bullet.GetComponent<Boomnana>().setup(gameObject);

        NetworkServer.Spawn(bullet);
    }
}
