using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BOOM_decal : NetworkBehaviour {

	// Use this for initialization
	void Start () {
        transform.rotation = Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
    }
}
