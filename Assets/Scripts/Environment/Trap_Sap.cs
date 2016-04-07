using UnityEngine;
using UnityEngine.Networking;

public class Trap_Sap : Trap {
    float amberStunTime = 60;

    void OnTriggerEnter(Collider _col) {
        if (_col.tag == "Player") {
            _col.transform.GetComponent<PlayerStats>().Slow(true);
            //Collider[] hitColliders = Physics.OverlapSphere(transform.position, transform.localScale.x * 0.1f);
            //foreach (Collider _collider in hitColliders) 
            //    if (_collider.GetComponent<Waterz>()) 
            //        Solidify();
            SpawnDripper(_col.transform);
        } //else if (_col.GetComponent<Waterz>()) { Solidify(); }
    }

    void OnTriggerStay(Collider _col) {
        if (_col.tag == "Player") {
            _col.transform.GetComponent<PlayerStats>().Slow(true);
        }
    }
    //void OnDestroy() { }
    
    void SpawnDripper(Transform trans) {
        if (!isServer)
            return;
        GameObject dripper = (GameObject)Instantiate(
            Resources.Load("Prefabs/Environments/ParticleSystems/Dripping") 
            as GameObject, trans.position, Quaternion.identity);

        dripper.GetComponent<DripScript>().SetParent(trans);

        NetworkServer.Spawn(dripper);
    }

    public void Solidify() {
        PlayerStats playerHit = null;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, transform.localScale.x * 1.1f);
        foreach (Collider _collider in hitColliders) {
            if (_collider.tag == "Player") { // Check if player
                playerHit = _collider.GetComponent<PlayerStats>();
                playerHit.Stun(amberStunTime);
            }
        }
        if (!isServer)
            return;
        GameObject bullet = (GameObject)Instantiate(
            Resources.Load("Prefabs/Environments/Amber"), playerHit == null ? transform.position : playerHit.transform.position,
            Quaternion.identity);
        bullet.GetComponent<Amber>().SetParent(playerHit.transform);
        Destroy(bullet, amberStunTime);
        NetworkServer.Spawn(bullet);
        Destroy(gameObject);
    }
}
