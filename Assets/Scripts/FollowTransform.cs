using UnityEngine;
using System.Collections;

public class FollowTransform : MonoBehaviour {
    public Transform trans;
    public int birdView = 100;

    // Use this for initialization
    void LateUpdate() {
        if (trans) transform.position = new Vector3(trans.position.x, trans.position.y + birdView, trans.position.z);
    }
}
