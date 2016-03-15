using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Dummy placeholder object for future pickup'ables
/// </summary>
public class Pickup : NetworkBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(new Vector3(0, .5f, 0));
	}
}
