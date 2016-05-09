using UnityEngine;
using System.Collections;

public class IgnoreCollision : MonoBehaviour {
    public bool player;
    public bool ability;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision _collision) {
        if (ability)
            if (_collision.collider.tag == "Ability") {
                Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
            }
        if (player)
            if (_collision.collider.tag == "Player") {
                Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
            }
    }
}
