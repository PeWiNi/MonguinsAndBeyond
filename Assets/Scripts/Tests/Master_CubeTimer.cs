using UnityEngine;
using System.Collections;

public class Master_CubeTimer : MonoBehaviour {
    public float timer;
	// Use this for initialization
	void Start () {
        timer = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetTime(float time) {
        timer = time;
    }
}
