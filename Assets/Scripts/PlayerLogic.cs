using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerLogic : NetworkBehaviour {

    PlayerStats stats;
    CharacterCamera cam;
    private float Speed = 5f; // Should JumpSpeed be a constant?
    [System.NonSerialized]
    public float horizAxis = 0f;
    [System.NonSerialized]
    public float vertAxis = 0f;
    [System.NonSerialized]
    public float jumpAxis = 0f;
    [System.NonSerialized]
    public float rotateAxis = 0f;

    Ability[] abilities;
    double castTime;

    public Collision collidedPlayer;

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
        cam = characterCam.GetComponent<CharacterCamera>();
        stats = GetComponent<PlayerStats>();
        Speed = stats? stats.speed : Speed;
    }

    void Update() {
        if(abilities == null || abilities.Length <= 0) {
            abilities = stats.abilities;
        }
        if (isLocalPlayer && !stats.isDead && castTime < Network.time && !stats.isStunned) { // if dead they cannot move
            // Send Critical Input
            horizAxis = Input.GetAxis("Horizontal");
            vertAxis = Input.GetAxis("Vertical");
            jumpAxis = Input.GetAxis("Jump");
            // Don't do this all the time ._. but only when new peeps connect
            foreach (HealthSlider hs in FindObjectsOfType<HealthSlider>())
                hs.setCamera(characterCam);

            #region abilities
            if (Input.GetKeyDown(KeyCode.E) && ((float)Network.time - abilities[0].timer) > abilities[0].cooldown) { //TODO make use of inputManager 
                //CmdDoFire(3.0f); // Dummy ability shooting bullets
                castTime = abilities[0].Trigger() + Network.time;
                abilities[0].timer = (float)Network.time;
            }
            if (Input.GetKeyDown(KeyCode.Q) && ((float)Network.time - abilities[1].timer) > abilities[1].cooldown) { //TODO make use of inputManager 
                castTime = abilities[1].Trigger() + Network.time;
                abilities[1].timer = (float)Network.time;
            }
            if (Input.GetKeyDown(KeyCode.F) && ((float)Network.time - abilities[2].timer) > abilities[2].cooldown) { //TODO make use of inputManager 
                castTime = abilities[2].Trigger() + Network.time;
                abilities[2].timer = (float)Network.time;
            }
            #endregion
        }
        if (stats.isDead || castTime > Network.time || stats.isStunned) { // Don't keep moving when dead~
            horizAxis = 0;
            vertAxis = 0;
            jumpAxis = 0;
        }
    }

    public void Simulate() {
        float speed;
        SetSpeed(out speed);
        // Movement
        transform.Translate(new Vector3(horizAxis, 0f, vertAxis) * speed * Time.fixedDeltaTime);
        // Jumping
        transform.Translate(new Vector3(0f, jumpAxis, 0f) * Speed * Time.fixedDeltaTime);

        if (Input.GetMouseButton(1) && !stats.isDead && !stats.isStunned) { // if dead they cannot turn their char around
            transform.rotation = Quaternion.Euler(0, cam.rotate.y, 0);
        }
        //transform.transform.Find("Cube").rotation = Quaternion.Euler(cam.rotate);
    }

    void SetSpeed(out float speed) {
        isWalking = !Input.GetKey(KeyCode.LeftShift);
        speed = isWalking ? Speed : Speed * 2;
    }

    void OnCollisionEnter(Collision _collision) {
        //Ignore collisions with other players
        if (_collision.collider.tag == "Player") {
            collidedPlayer = _collision;
            Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
        }
    }
}
