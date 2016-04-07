using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Amber : RotateMe {
    [SyncVar]
    Transform host;

    void Start () {
        Physics.IgnoreCollision(host.GetComponent<Collider>(), GetComponentInChildren<Collider>());
    }

	// Update is called once per frame
	void FixedUpdate () {
	    if (host != null) {
            transform.position = host.position;
        }
	}

    public void SetParent(Transform parent) {
        host = parent;
        transform.position = host.position;
    }
}
