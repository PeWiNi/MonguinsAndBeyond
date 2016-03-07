using UnityEngine;
using System.Collections;

public class Banana : Pickup {
    public int bananas = 1;

	void OnTriggerEnter(Collider _collider) {
        if(_collider.tag == "Player") {
            _collider.GetComponent<SyncInventory>().pickupBanana(bananas);
            Destroy(gameObject);
        }
    }
}
