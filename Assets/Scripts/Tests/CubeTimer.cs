using UnityEngine;
using System.Collections;

public class CubeTimer : MonoBehaviour {
    public Master_CubeTimer mct;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision _collision) {
        print("Timer: " + (Time.time - mct.timer));
        mct.SetTime(Time.time);
    }
}
