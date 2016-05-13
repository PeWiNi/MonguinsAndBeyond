using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Sap : Pickup {
    void OnTriggerEnter(Collider _collider) {
        if (!canCollide)
            return;
        if (_collider.tag == "Player") {
            _collider.GetComponent<SyncInventory>().pickupSap(1);

            if (isServer) {
                GameObject particles = (GameObject)Instantiate(
                    Resources.Load("Prefabs/Environments/ParticleSystems/SapPS"),
                    transform.position, Quaternion.Euler(270f, 0, 0));
                Destroy(particles, 10);
                NetworkServer.Spawn(particles);
            }

            Destroy(gameObject);
        }
    }
}
