using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class FroggyTrack : NetworkBehaviour {
    [SerializeField]
    GameObject Track;
    [SyncVar]
    bool statusOn = false;
    bool on = false;
    //[SerializeField]
    //Material frogOn;
    //[SerializeField]
    //Material frogOff;

    [SerializeField]
    SkinnedMeshRenderer frog;
	
	// Update is called once per frame
	void Update () {
	    if(on != statusOn) {
            on = statusOn;
            //frog.material = statusOn ? frogOn : frogOff;
            Track.SetActive(statusOn);
        }
	}

    void OnTriggerEnter(Collider _col) {
        if(isServer && _col.tag == "Player") {
            statusOn = !statusOn;
        }
    }
}
