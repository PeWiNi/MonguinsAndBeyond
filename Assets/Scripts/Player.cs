using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {

    public float WalkSpeed = 5f;
    public float RunSpeed = 10f;
    [System.NonSerialized]
    public float horizAxis = 0f;
    [System.NonSerialized]
    public float vertAxis = 0f;
    [System.NonSerialized]
    public float jumpAxis = 0f;
    [System.NonSerialized]
    public float rotateAxis = 0f;

    bool isWalking;

    //[SerializeField]
    //private UnityStandardAssets.Characters.FirstPerson.MouseLook m_MouseLook;

    [SerializeField]
    Camera characterCam;
    [SerializeField]
    AudioListener audioListener;

    void Start() {
        if (isLocalPlayer) {
            // Disable and Enable Cameras / AudioListener
            GameObject.Find("Main Camera").SetActive(false);
            characterCam.enabled = true;
            audioListener.enabled = true;

            //m_MouseLook.Init(transform, characterCam.transform);
        }
    }

    void Update() {
        if (isLocalPlayer) {
            // Send Critical Input
            horizAxis = Input.GetAxis("Horizontal");
            vertAxis = Input.GetAxis("Vertical");
            jumpAxis = Input.GetAxis("Jump");
            rotateAxis = Input.GetAxis("Rotate");
        }
    }

    public void Simulate() {
        float speed;
        SetSpeed(out speed);
        // Movement
        transform.Translate(new Vector3(horizAxis, jumpAxis, vertAxis) * speed * Time.fixedDeltaTime);

        // Rotate View
        transform.Rotate(new Vector3(0, rotateAxis, 0));
        //m_MouseLook.LookRotation(transform, characterCam.transform);
    }

    void SetSpeed(out float speed) {
        isWalking = !Input.GetKey(KeyCode.LeftShift);
        speed = isWalking ? WalkSpeed : RunSpeed;
    }

    void OnCollisionEnter(Collision _collision) {
        //Ignore collisions with other players
        if (_collision.collider.tag == "Player") {
            Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
        }
    }
}
