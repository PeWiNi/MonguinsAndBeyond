using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Class for spawning an object in the scene using the server
/// 
/// NB: Should be ServerOnly
/// </summary>
public class PickupSpawner : NetworkBehaviour {
    public GameObject prefab;
    public float spawnTime = 5f;
    public Vector3 SpawnPosition;

	// Use this for initialization
	void Start () {
        if(isServer) {
            if(SpawnPosition == new Vector3()) SpawnPosition = new Vector3(0, .75f, 0);
            //InvokeRepeating("RepeatSpawn", 0f, spawnTime);
            Spawn(SpawnPosition);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// Simple spawn method spawning the given prefab a little above the transform
    /// Will not spawn anything if its position is obstructed
    /// 
    /// Works well if using InvokeRepeating
    /// </summary>
    void Spawn(Vector3 pos, float destroy = 0) {
        GameObject go = (GameObject)Instantiate(
            prefab, transform.position + pos, prefab.transform.rotation);
        if (destroy > 0) Destroy(go, destroy);
        go.GetComponent<Pickup>().SetSpawner(this);
        NetworkServer.Spawn(go);
    }

    void RepeatSpawn() {
        CheckForStuff(SpawnPosition);
        Spawn(SpawnPosition);
    }

    void CheckForStuff(Vector3 pos) {
        Collider[] hitColliders = Physics.OverlapSphere(pos, .25f);
        foreach (Collider c in hitColliders)
            //if(!c.isTrigger)
            return;
    }

    public void TriggerSpawn() {
        StartCoroutine(TriggerSpawnRoutine());
    }

    IEnumerator TriggerSpawnRoutine() {
        yield return new WaitForSeconds(spawnTime);
        Spawn(SpawnPosition);
    }
}
