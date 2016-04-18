using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SyncInventory : NetworkBehaviour {
    Inventory inventory;
    
    public void pickupBanana(int count = 1) {
        if(isLocalPlayer) {
            inventory.pickupBanana(count);
            pickupEffect("Banana");
        }
    }
    public void pickupSticks(int count = 1) {
        if (isLocalPlayer) { 
            inventory.pickupSticks(count);
            pickupEffect("Stick");
        }
    }
    public void pickupSap(int count = 1) {
        if (isLocalPlayer) {
            inventory.pickupSap(count);
            pickupEffect("Sap");
        }
    }
    public void pickupLeaf(int count = 1) {
        if (isLocalPlayer) {
            inventory.pickupLeaf(count);
            pickupEffect("Leaf");
        }
    }
    public void pickupBerry(int value) {
        if (isLocalPlayer) {
            int quality = inventory.pickupBerry(value, GetComponent<PlayerStats>().wisdom);
            veryBerryThings(quality);
        }
    }
    public void pickupBerry(Herb.Condition value) {
        if (isLocalPlayer) {
            int quality = inventory.pickupBerry(value);
            veryBerryThings(quality);
        }
    }

    void veryBerryThings(int berryType) {
        GameObject pickup = (GameObject)Instantiate(Resources.Load("Prefabs/Environments/Collectables/Herb"), transform.position, transform.rotation);
        string berry = berryType == 1 ? "BerryG" : berryType == 2 ? "BerryB" : "BerryR";
        pickup.GetComponent<Herb>().ChangeProperties(berry, GetComponent<PlayerStats>().team);
        pickup.GetComponent<Pickup>().owner = transform;
        //pickup.transform.FindChild("Berry").GetComponent<MeshRenderer>().materials[0] = Resources.Load("Materials/berry" + (berryType == 1 ? "_good" : berryType == 2 ? "_bad" : "_neutral")) as Material;
        //pickup.transform.FindChild("Berry").GetComponent<MeshRenderer>().materials[1] = Resources.Load("Materials/berry_leaf") as Material;
        //pickup.transform.FindChild("Berry").GetComponent<MeshRenderer>().material = Resources.Load("Materials/berry" + (berryType == 1 ? "_good" : berryType == 2 ? "_bad" : "_neutral")) as Material;
        pickup.GetComponent<Pickup>().makeMoveGuy(inventory.transform.FindChild(berry), GetComponentInChildren<Camera>());
        StartCoroutine(pickupFlashEffect(berry));
    }

    void pickupEffect(string type) {
        GameObject pickup = (GameObject)Instantiate(Resources.Load("Prefabs/Environments/Collectables/" + type), transform.position, transform.rotation);
        pickup.GetComponent<Pickup>().makeMoveGuy(inventory.transform.FindChild(type), GetComponentInChildren<Camera>());
        StartCoroutine(pickupFlashEffect(type));
    }

    IEnumerator pickupFlashEffect(string type) {
        Transform button = inventory.transform.FindChild(type);
        while(button.localScale.x < 1.2f) {
            button.localScale *= 1.01f;
            yield return new  WaitForFixedUpdate();
        } while(button.localScale.x > 1f) {
            button.localScale /= 1.01f;
            yield return new WaitForFixedUpdate();
        }
        yield return null;
    }

    public void setInventory(Inventory i) {
        inventory = i;
    }

    [Command]
    public void CmdUseLeaf() {
        gameObject.GetComponent<Camouflage>().BeginCamouflage();

        GameObject particles = (GameObject)Instantiate(
            Resources.Load("Prefabs/Environments/ParticleSystems/LeavesPS"),
            transform.position, Quaternion.Euler(270f, 0, 0));
        Destroy(particles, 5);
        NetworkServer.Spawn(particles);
    }

    public void DropItem(string go, Vector3 position, float duration) {
        GetComponent<Animator>().SetTrigger("Dropping");
        CmdSpawnItem(go, position, duration);
    }

    [Command]
    public void CmdSpawnItem(string go, Vector3 position, float duration) {
        Vector3 spawnPos = transform.position + ((transform.localScale.x * 2) * transform.forward);
        if (position != new Vector3())
            spawnPos = position;

        GameObject banana;
        if (go == "BerryR" || go == "BerryG" || go == "BerryB")
            banana = Resources.Load("Prefabs/Environments/Collectables/Herb") as GameObject;
        else
            banana = Resources.Load("Prefabs/Environments/Collectables/" + go) as GameObject;

        GameObject bananaNfunzies = (GameObject)Instantiate(
            banana, spawnPos, banana.transform.rotation);

        if (go == "BerryR" || go == "BerryG" || go == "BerryB") 
            bananaNfunzies.GetComponent<Herb>().ChangeProperties(go, GetComponent<PlayerStats>().team);

        bananaNfunzies.GetComponent<Pickup>().owner = transform;

        if (duration > 0)
            Destroy(bananaNfunzies, duration);
        NetworkServer.Spawn(bananaNfunzies);
    }

    public int HealForceBerryConsume() {
        int berryConsumed = 0;
        if(inventory.berryGCount == 1) {
            inventory.useBerry("BerryG");
            berryConsumed = 1;
        } else if (inventory.berryGCount > 1) {
            while(berryConsumed < 4 && inventory.berryGCount > 1) {
                inventory.useBerry("BerryG");
                berryConsumed++;
            }
        }
        return berryConsumed;
    }

    public bool ThrowPoisonBerryConsume() {
        return inventory.useBerry("BerryB");
    }
}
