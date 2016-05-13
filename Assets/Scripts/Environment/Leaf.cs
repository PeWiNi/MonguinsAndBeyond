using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Camouflage Leaf-pickup that will camouflage the player who 'picks it up'
/// </summary>
public class Leaf : Pickup {
    void OnTriggerEnter(Collider _collider) {
        if (!canCollide)
            return;
        if (_collider.tag == "Player") {
            _collider.GetComponent<SyncInventory>().pickupLeaf(1);
            Destroy(gameObject);
        }
    }
}
