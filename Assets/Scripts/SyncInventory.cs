using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SyncInventory : NetworkBehaviour {
    Inventory inventory;
    
    public void pickupBanana(int count = 1) {
        if(isLocalPlayer)
            inventory.pickupBanana(count);
    }
    public void pickupSticks(int count = 1) {
        if (isLocalPlayer)
            inventory.pickupSticks(count);
    }

    public void setInventory(Inventory i) {
        inventory = i;
    }

    [Command]
    public void CmdSpawnItem(string go, Vector3 position, float duration) {
        Vector3 spawnPos = transform.position + ((transform.localScale.x * 2) * transform.forward);
        if (position != new Vector3())
            spawnPos = position;
        GameObject banana = Resources.Load(go) as GameObject;
        GameObject bananaNfunzies = (GameObject)Instantiate(
            banana, spawnPos, banana.transform.rotation);
        if (duration > 0)
            Destroy(bananaNfunzies, duration);

        NetworkServer.Spawn(bananaNfunzies);
    }
}
