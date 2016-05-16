using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Class for spawning an object in the scene using the server
/// 
/// NB: Should be ServerOnly
/// </summary>
public class PickupSpawner : NetworkBehaviour {
    public GameObject spawnable;
    public Vector3 SpawnPosition;

    [SyncVar]
    public float spawnTime = 5f;
    [SyncVar]
    public float value = 0;
    float increment = 3;
    bool pickedUp;

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
            spawnable, transform.position + pos, spawnable.transform.rotation);
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
        pickedUp = true;
        StartCoroutine(TriggerSpawnRoutine());
    }

    IEnumerator TriggerSpawnRoutine() {
        yield return new WaitForSeconds(spawnTime + value);
        Spawn(SpawnPosition);
        pickedUp = false;
        StartCoroutine(makeIncrement());
    }

    IEnumerator makeIncrement() {
        value += increment; // Discourage camping by increasing the spawntime whenever picked up
        yield return new WaitForSeconds(spawnTime + value);
        while (!pickedUp && value > 0) { // Decrement the spawnTime over time if they leave it alone
            value -= increment;
            yield return new WaitForSeconds(spawnTime + value);
        }
        value = value < 0 ? 0 : value;
        yield return null;
    }
}
