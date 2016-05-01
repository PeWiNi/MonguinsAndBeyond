using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class HealWave : NetworkBehaviour {

    Animator animator;
    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
        animator.SetTrigger("HEAL");
        //animator.Play("healWave_maybe");
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
