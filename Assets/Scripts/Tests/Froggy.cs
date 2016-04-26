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

    [SerializeField]
    bool immovable;

    GameObject target;
    Vector3 boundPoint;
    bool homeBound = false;

    // Use this for initialization
    void Start() {
        animator = GetComponent<Animator>();
        boundPoint = transform.position;
    }

    // Update is called once per frame
    void Update() {
        #region Idle
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !isIdle) {
            isIdle = true;
            currentTime = (float)Network.time + timeToIdle;
        } else if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && isIdle) {
            isIdle = false;
        } if (isIdle) {
            CheckIdle();
        }
        #endregion
    }
    
    void OnAnimatorMove() {
        if (!immovable) {
            if (target) {
                float dist = Vector3.Distance(transform.position, target.transform.position);
                transform.LookAt(target.transform);
                float speed = (dist / 2) * Time.deltaTime;
                Vector3 newPos = Vector3.MoveTowards(transform.position, target.transform.position, speed);
                animator.SetFloat("speed", speed);
                transform.position = newPos;
                if (dist > 10) {
                    homeBound = true;
                    target = null;
                }
            } if (homeBound) {
                Vector3 newPos = Vector3.MoveTowards(transform.position, boundPoint, 10 * Time.deltaTime);
                animator.SetFloat("speed", 10 * Time.deltaTime);
                transform.position = newPos;
            }
            /*
            if (animator) {
                Vector3 newPosition = transform.position;
                newPosition.z += animator.GetFloat("Runspeed") * Time.deltaTime;
                transform.position = newPosition;
            }
            */
        }
    }

    void CheckIdle() {
        if ((float)Network.time > currentTime) {
            int rand = Random.Range(0, 20) + 1;
            if (rand <= 5) 
                animator.SetTrigger("idleLookRight");
            else if (rand <= 10) 
                animator.SetTrigger("idleLookLeft");
            else if (rand <= 15) 
                animator.SetTrigger("idleLookAround");
            else if (rand > 20) {
                animator.SetTrigger("idleJump");
                // Make it physically jump (addForce or wtv)
            }
            currentTime = (float)Network.time + timeToIdle + Random.Range(0, 5);
        }
    }

    void OnCollisionEnter(Collision _col) {
        if (_col.gameObject.tag == "Player") {
            if(!target) {
                target = _col.gameObject;
            }
            Physics.IgnoreCollision(_col.collider, GetComponent<Collider>());
        }
    }
}