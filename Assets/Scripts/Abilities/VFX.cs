using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

class VFX : NetworkBehaviour
{
    [SyncVar]
    Transform owner;
    [SyncVar]
    Vector3 offset;
    [SyncVar]
    Vector3 eulerRot;
    [SyncVar]
    bool follow;

    public void Setup(Transform owner, float killTimer, bool followMe, Vector3 offset = new Vector3(), Vector3 eulerRotation = new Vector3()) {
        this.owner = owner;
        this.offset = offset;
        eulerRot = eulerRotation;
        follow = followMe;
        transform.position = owner.position + offset;
        transform.eulerAngles = eulerRot;
        Destroy(gameObject, killTimer);
    }

    void Update() {
        if (transform.eulerAngles != eulerRot)
            transform.eulerAngles = eulerRot;
        if(follow)
            transform.position = owner.position + offset;
        if(eulerRot == new Vector3()) transform.rotation = owner.rotation;
    }
}