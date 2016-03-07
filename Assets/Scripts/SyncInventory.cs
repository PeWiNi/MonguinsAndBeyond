using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SyncInventory : NetworkBehaviour {
    Inventory inventory;
    
    public void pickupBanana(int count = 1) {
        if(isLocalPlayer)
            inventory.pickupBanana(count);
    }

    public void setInventory(Inventory i) {
        inventory = i;
    }
}
