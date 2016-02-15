using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ShootBullet : Ability {
    public GameObject bulletPrefab; //= Resources.Load("Prefabs/Bullet") as GameObject;
    float lifeTime = 3.0f;

    public override double Trigger() {
        //GetComponentInParent<PlayerStats>().CmdDoFire(3.0f);
        CmdDoFire();
        return castTime;
    }

    [Command]
    void CmdDoFire() {
        GameObject bullet = (GameObject)Instantiate(
            bulletPrefab, transform.position + (transform.localScale.x * transform.forward),
            Quaternion.identity);

        bullet.GetComponent<Bullet>().setOwner(team);

        var bullet3D = bullet.GetComponent<Rigidbody>();
        bullet3D.velocity = transform.forward * 5f;
        Destroy(bullet, lifeTime);

        NetworkServer.Spawn(bullet);
    }
}
