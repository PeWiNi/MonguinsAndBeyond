using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

class PukeVX : NetworkBehaviour {
    [SyncVar] Transform owner;

    public void Setup(Transform owner, float killTimer) {
        this.owner = owner;
        Destroy(gameObject, killTimer);
    }

    void Update() {
        transform.position = owner.position + Vector3.up * (owner.localScale.y * 1.15f);
        transform.rotation = owner.rotation;
    }
}