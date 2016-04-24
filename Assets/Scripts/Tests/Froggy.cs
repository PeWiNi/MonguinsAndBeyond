using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(Animator))]
public class Froggy : NetworkBehaviour {

    Animator animator;

    //Idle state variables.
    public float timeToIdle = 5;
    float currentTime = 0f;
    bool isIdle = false;

    // Use this for initialization
    void Start() {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        #region Idle
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !isIdle) {
            isIdle = true;
            currentTime = (float)Network.time + timeToIdle;
            CheckIdle();
        } else if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && isIdle) {
            isIdle = false;
        } if (isIdle) {
            CheckIdle();
        }
        #endregion
    }
    
    /*void OnAnimatorMove() {
        Animator animator = GetComponent<Animator>();

        if (animator) {
            Vector3 newPosition = transform.position;
            newPosition.z += animator.GetFloat("Runspeed") * Time.deltaTime;
            transform.position = newPosition;
        }
    }*/

    void CheckIdle() {
        if ((float)Network.time > currentTime) {
            int rand = Random.Range(0, 20) + 1;
            if (rand <= 5) 
                animator.SetTrigger("idleLookRight");
            else if (rand <= 10) 
                animator.SetTrigger("idleLookLeft");
            else if (rand <= 15) 
                animator.SetTrigger("idleLookAround");
            else if (rand > 20)
                animator.SetTrigger("idleJump");
            currentTime = (float)Network.time + timeToIdle + Random.Range(0, 5);
        }
    }

    void OnCollisionEnter(Collision _col) {
        if (_col.gameObject.tag == "Player") {
            Physics.IgnoreCollision(_col.collider, GetComponent<Collider>());
        }
    }
}