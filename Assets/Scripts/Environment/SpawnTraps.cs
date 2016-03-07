using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SpawnTraps : NetworkBehaviour {
    // ADD PREVIEWS of traps
    // Interconnectivity with CharacterCamera (disable while putting traps so you can properly place the trap)
    GameObject bananaTrap;

	// Use this for initialization
	void Start () {
        bananaTrap = Resources.Load("Prefabs/Trap_BananaIsland") as GameObject;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Slippery() {
        Vector3 spawnPos = transform.position + ((transform.localScale.x + 10) * transform.forward);
        spawnPos.y = 2;
        GameObject banana = (GameObject)Instantiate(
            bananaTrap, spawnPos,
            Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));

        NetworkServer.Spawn(banana);
    }
}
