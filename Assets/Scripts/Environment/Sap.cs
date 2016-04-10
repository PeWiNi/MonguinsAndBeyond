using UnityEngine;
using System.Collections;

public class Sap : Pickup {
    public int sap = 1;

    void OnTriggerEnter(Collider _collider) {
        if (!canCollide)
            return;
        _collider.GetComponent<SyncInventory>().pickupSap(sap);
        Destroy(gameObject);
    }
}
