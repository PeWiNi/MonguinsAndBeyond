using UnityEngine;
using System.Collections;

public class FollowTransform : MonoBehaviour {
    public Transform trans;

	// Use this for initialization
	void LateUpdate() {
        if (trans) transform.position = new Vector3(trans.position.x, trans.position.y + 10, trans.position.z);
    }
}
