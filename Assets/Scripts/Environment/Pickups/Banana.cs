using UnityEngine;
using System.Collections;

public class Banana : Pickup {
	void OnTriggerEnter(Collider _collider) {
        if (!canCollide)
            return;
        if (_collider.tag == "Player") {
            _collider.GetComponent<SyncInventory>().pickupBanana(1);
            Destroy(gameObject);
        }
    }
}
