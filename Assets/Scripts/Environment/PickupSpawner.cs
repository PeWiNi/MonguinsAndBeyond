using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PickupSpawner : NetworkBehaviour {
    public GameObject prefab;
    public float repeatTime = 5f;

	// Use this for initialization
	void Start () {
        InvokeRepeating("SpawnLeaf", 0f, repeatTime);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void SpawnLeaf() {
        Vector3 pos = transform.position + new Vector3(0, .75f, 0);
        Collider[] hitColliders = Physics.OverlapSphere(pos, .25f);
        if (hitColliders.Length > 0)
            return;
        GameObject leaf = (GameObject)Instantiate(
            prefab, pos, Quaternion.identity);
        //Destroy(leaf, repeatTime);
        NetworkServer.Spawn(leaf);
    }
}
