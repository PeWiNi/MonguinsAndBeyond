using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class RotateMe : NetworkBehaviour {
    [SyncVar]
    public float x = 0;
    [SyncVar]
    public float y = 0;
    [SyncVar]
    public float z = 0;

    [SyncVar]
    public bool rotateAroundSelf;

	// Use this for initialization
	void Start () {
        transform.rotation = Quaternion.Euler(x, y, z);
    }

    void Update() {
        if(rotateAroundSelf)
            transform.Rotate(new Vector3(0, .5f, 0));
    }

    public void SetRotation(Quaternion rot) {
        x = rot.eulerAngles.x;
        y = rot.eulerAngles.y;
        z = rot.eulerAngles.z;
        transform.rotation = rot;
    }
}
