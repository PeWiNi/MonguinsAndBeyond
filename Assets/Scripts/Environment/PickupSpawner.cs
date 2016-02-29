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
    public float repeatTime = 5f;

	// Use this for initialization
	void Start () {
        InvokeRepeating("Spawn", 0f, repeatTime);
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
    void Spawn() {
        Vector3 pos = transform.position + new Vector3(0, .75f, 0);
        Collider[] hitColliders = Physics.OverlapSphere(pos, .25f);
        if (hitColliders.Length > 0)
            return;
        GameObject go = (GameObject)Instantiate(
            prefab, pos, Quaternion.identity);
        //Destroy(go, repeatTime);
        NetworkServer.Spawn(go);
    }
}
