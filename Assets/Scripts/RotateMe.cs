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
    [SyncVar]
    public bool randomRotation;

    // Use this for initialization
    void Start () {
        transform.rotation = Quaternion.Euler(x, y, z);
        if (randomRotation) {
            transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        }
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
