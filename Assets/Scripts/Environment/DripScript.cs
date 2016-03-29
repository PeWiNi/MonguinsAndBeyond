using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DripScript : RotateMe {
    [SyncVar]
    Transform host;

	// Update is called once per frame
	void FixedUpdate () {
	    if (host != null) {
            transform.position = host.position + new Vector3(0f, .5f, -0.1f);
            if (!host.GetComponent<PlayerStats>().isSlowed)
                Destroy(gameObject);
        }
	}

    public void SetParent(Transform parent) {
        host = parent;
    }
}
