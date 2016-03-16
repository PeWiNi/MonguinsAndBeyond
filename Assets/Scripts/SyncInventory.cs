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
    public void pickupBerry(int value) {
        if (isLocalPlayer) {
            inventory.pickupBerry(value, GetComponent<PlayerStats>().wisdom);
        }
    }

    public void setInventory(Inventory i) {
        inventory = i;
    }

    [Command]
    public void CmdSpawnItem(string go, Vector3 position, float duration) {
        Vector3 spawnPos = transform.position + ((transform.localScale.x * 2) * transform.forward);
        if (position != new Vector3())
            spawnPos = position;

        GameObject banana;
        if (go == "BerryR" || go == "BerryG" || go == "BerryB")
            banana = Resources.Load("Prefabs/Herb") as GameObject;
        else
            banana = Resources.Load("Prefabs/" + go) as GameObject;

        GameObject bananaNfunzies = (GameObject)Instantiate(
            banana, spawnPos, banana.transform.rotation);

        if (go == "BerryR" || go == "BerryG" || go == "BerryB") 
            bananaNfunzies.GetComponent<Herb>().ChangeProperties(go);

        if (duration > 0)
            Destroy(bananaNfunzies, duration);
        NetworkServer.Spawn(bananaNfunzies);
    }
}
