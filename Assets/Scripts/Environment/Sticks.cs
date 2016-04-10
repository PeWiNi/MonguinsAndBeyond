using UnityEngine;
using System.Collections;

public class Sticks : Pickup {
    public int sticks = 1;
    void OnTriggerEnter(Collider _collider) {
        if (!canCollide)
            return;
        if (_collider.tag == "Player") {
            _collider.GetComponent<SyncInventory>().pickupSticks(sticks);
            Destroy(gameObject);
        }
    }
}
