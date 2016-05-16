using UnityEngine;
using System.Collections;

public class Sticks : Pickup {
    void OnTriggerEnter(Collider _collider) {
        if (!canCollide)
            return;
        if (_collider.tag == "Player") {
            _collider.GetComponent<SyncInventory>().pickupSticks(1);
            Destroy(gameObject);
        }
    }
}
