using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerLogic : NetworkBehaviour {

    PlayerStats stats;
    OrbitCharacter cam;
    private float Speed = 5f; // Should JumpSpeed be a constant?
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
        cam = transform.Find("Character").GetComponent<OrbitCharacter>();
        stats = GetComponent<PlayerStats>();
        Speed = stats? stats.speed : Speed;
    }

    void Update() {
        if (isLocalPlayer) {
            // Send Critical Input
            horizAxis = Input.GetAxis("Horizontal");
            vertAxis = Input.GetAxis("Vertical");
            jumpAxis = Input.GetAxis("Jump");
        }
    }

    public void Simulate() {
        float speed;
        SetSpeed(out speed);
        // Movement
        transform.Translate(new Vector3(horizAxis, 0f, vertAxis) * speed * Time.fixedDeltaTime);
        // Jumping
        transform.Translate(new Vector3(0f, jumpAxis, 0f) * Speed * Time.fixedDeltaTime);

        transform.rotation = cam.rotate;
    }

    void SetSpeed(out float speed) {
        isWalking = !Input.GetKey(KeyCode.LeftShift);
        speed = isWalking ? Speed : Speed * 2;
    }

    void OnCollisionEnter(Collision _collision) {
        //Ignore collisions with other players
        if (_collision.collider.tag == "Player") {
            Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
        }
    }
}
