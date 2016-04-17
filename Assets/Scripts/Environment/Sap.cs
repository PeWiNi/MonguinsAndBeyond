using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Sap : Pickup {
    public int sap = 1;

    void OnTriggerEnter(Collider _collider) {
        if (!canCollide)
            return;
        _collider.GetComponent<SyncInventory>().pickupSap(sap);

        if(isServer) {
            GameObject particles = (GameObject)Instantiate(
                Resources.Load("Prefabs/Environments/ParticleSystems/SapPS"),
                transform.position, Quaternion.Euler(270f, 0, 0));
            Destroy(particles, 10);
            NetworkServer.Spawn(particles);
        }

        Destroy(gameObject);
    }
}
