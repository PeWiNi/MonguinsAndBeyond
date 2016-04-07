using UnityEngine;
using System.Collections;

public class IgnoreCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision _collision) {
        if (_collision.collider.tag == "Player") {
            Collision collidedPlayer = _collision;
            Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
        }
    }
}
