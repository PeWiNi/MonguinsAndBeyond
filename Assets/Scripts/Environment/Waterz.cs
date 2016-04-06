using UnityEngine;
using System.Collections;

public class Waterz : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider _collider) {
        print("In Water");
        if (_collider.tag == "Player") {
            _collider.GetComponent<Rigidbody>().useGravity = false;
            _collider.GetComponent<Rigidbody>().velocity = new Vector3();
        }
    }

    void OnTriggerStay(Collider _collider) {
        if (_collider.tag == "Player") {
            _collider.GetComponent<PlayerLogic>().StartSwimming();
            Vector3 pos = _collider.transform.position + (_collider.transform.localScale.y + .5f) * _collider.transform.up;
            pos = new Vector3(pos.x,
                Mathf.Lerp(pos.y, transform.position.y + transform.localScale.y, Time.deltaTime),
                pos.z);
            _collider.transform.position = pos - (_collider.transform.localScale.y + .5f) * _collider.transform.up;
            //_collider.transform.position = new Vector3(_collider.transform.position.x,
            //    Mathf.Lerp(_collider.transform.position.y, transform.position.y + transform.localScale.y, Time.deltaTime),
            //    _collider.transform.position.z);// - (_collider.transform.localScale.y + .5f) * _collider.transform.up;
        }

    }

    void OnTriggerExit(Collider _collider) {
        if (_collider.tag == "Player") {
            _collider.GetComponent<Rigidbody>().useGravity = true;
        }
    }
}
