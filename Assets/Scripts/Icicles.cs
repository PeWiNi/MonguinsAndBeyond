using UnityEngine;
using System.Collections;

public class Icicles : MonoBehaviour {

    int numberOfJumps = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision col)
    {
        numberOfJumps++;
        print(numberOfJumps);
        col.transform.parent = transform;
    }

    void OnCollisionStay(Collision col)
    {

    }

    void OnCollisionExit(Collision col)
    {
        print("Off we go");
        col.transform.parent = null;
    }
}
