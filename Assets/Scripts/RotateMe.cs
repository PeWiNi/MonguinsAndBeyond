using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class RotateMe : NetworkBehaviour {
    public float x = 0;
    public float y = 0;
    public float z = 0;

	// Use this for initialization
	void Start () {
        transform.rotation = Quaternion.Euler(x, y, z);
    }
}
