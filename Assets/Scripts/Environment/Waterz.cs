using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Waterz : NetworkBehaviour {

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
            _collider.GetComponent<Rigidbody>().velocity = new Vector3(); // Figure a better solution to them flying away
            //Amberfy - THOUGHT: Move to OnTriggerStay and grant a couple seconds immunity (to not be stun-locked by someone spamming saptraps while you are in the water)
            if(_collider.GetComponent<PlayerStats>().isSlowed && !_collider.GetComponent<PlayerStats>().isStunned)
                AmberIt(_collider.GetComponent<PlayerStats>());
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

    void AmberIt(PlayerStats ps) {
        float stunTime = EventManager.amberStunTime * ps.sapModifier;
        ps.Stun(stunTime);
        if (!isServer)
            return;
        GameObject bullet = (GameObject)Instantiate(
            Resources.Load("Prefabs/Environments/Amber"), ps.transform.position,
            Quaternion.identity);
        bullet.GetComponent<Amber>().SetParent(ps.transform);
        Destroy(bullet, stunTime);
        NetworkServer.Spawn(bullet);
    }
}
