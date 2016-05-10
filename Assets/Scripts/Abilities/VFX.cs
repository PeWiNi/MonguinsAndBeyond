using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

class VFX : NetworkBehaviour
{
    [SyncVar]
    Transform owner;
    Vector3 offset;

    public void Setup(Transform owner, float killTimer, Vector3 offset) {
        this.owner = owner;
        this.offset = offset;
        Destroy(gameObject, killTimer);
    }

    void Update() {
        transform.position = owner.position + offset;
        transform.rotation = owner.rotation;
    }
}