using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Camouflage Leaf-pickup that will camouflage the player who 'picks it up'
/// </summary>
public class Leaf : Pickup {
    public int leaves = 1;

    void OnTriggerEnter(Collider _collider) {
        if (!canCollide)
            return;
        _collider.GetComponent<SyncInventory>().pickupLeaf(leaves);

        Destroy(gameObject);
    }
}
