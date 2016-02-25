using UnityEngine;
using System.Collections;

public class Leaf : Pickup {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider _collider) {
        _collider.gameObject.GetComponent<Camouflage>().CmdBeginCamouflage();
        Destroy(gameObject);
    }
}
