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

    bool isWalking;
    bool jump;

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
            GetComponent<CharacterController>().enabled = true;
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
        }
    }

    public void Simulate() {
        float speed;
        GetInput(out speed);
        // Movement
        transform.Translate(new Vector3(horizAxis, 0, vertAxis) * speed * Time.fixedDeltaTime);
        // Rotate View
        //m_MouseLook.LookRotation(transform, characterCam.transform);
    }

    void GetInput(out float speed) {

        isWalking = !Input.GetKey(KeyCode.LeftShift);
        speed = isWalking ? WalkSpeed : RunSpeed;
    }
}
