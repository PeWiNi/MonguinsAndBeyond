using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Class in charge of parsing user input
/// </summary>
public class PlayerLogic : NetworkBehaviour
{

    PlayerStats stats;
    CharacterCamera cam;
    private float Speed = 5f;
    private float jumpSpeed = 4f;
    [System.NonSerialized]
    public float horizAxis = 0f;
    [System.NonSerialized]
    public float vertAxis = 0f;
    [System.NonSerialized]
    public bool jump;
    //public float jumpAxis = 0f;
    [System.NonSerialized]
    public float rotateAxis = 0f;

    Ability[] abilities;
    double castTime;

    bool dblJump = false;
    //bool isWalking;

    [SyncVar]
    Vector3 externalForce;

    public bool isSwimming;
    public float drownDepth = 5;
    float drownTimer;
    float drownTime = 15f;

    //[SerializeField]
    //private UnityStandardAssets.Characters.FirstPerson.MouseLook m_MouseLook;

    [SerializeField]
    Camera characterCam;
    [SerializeField]
    AudioListener audioListener;

    Animator anim;
    public bool isFlying;
    public bool isFlyingFrontHands;

    void Start() {
        // Only enable sound and camera for player if it's the local player
        if (isLocalPlayer) {
            // Disable and Enable Cameras / AudioListener
            try { GameObject.Find("Main Camera").SetActive(false); }
            catch { }
            characterCam.enabled = true;
            audioListener.enabled = true;
            //characterCam.transform.GetChild(0).GetComponent<Camera>().enabled = true;
            //m_MouseLook.Init(transform, characterCam.transform);
        }
        cam = characterCam.GetComponent<CharacterCamera>();
        stats = GetComponent<PlayerStats>();
        Speed = stats ? stats.speed : Speed; // Only use speed from playerStats if it is not null
        anim = GetComponent<Animator>();
    }

    void Update() {
        if (abilities == null || abilities.Length <= 0) {
            abilities = stats.abilities;
        }
        if (isLocalPlayer && castTime < Network.time && stats.CanIMove()) { // if dead they cannot move
            // Send Critical Input
            horizAxis = Input.GetAxis("Horizontal");
            vertAxis = Input.GetAxis("Vertical");
            jump = Input.GetButtonDown("Jump");
            //jumpAxis = Input.GetAxis("Jump");
            // Don't do this all the time ._. but only when new peeps connect
            foreach (HealthSlider hs in FindObjectsOfType<HealthSlider>()) // Find HealthSliders of all players and make them point towards you
                hs.setCamera(characterCam, stats.team == hs.transform.parent.GetComponentInParent<PlayerStats>().team);
            if (/*transform.position.y < 25 &&*/ !GetComponent<SpawnTraps>().isPlacing && !GetComponent<Aim>().aiming) {
                #region abilities
                if (Input.GetKeyDown(KeyCode.Q) && !abilities[0].OnCooldown()) { //TODO make use of inputManager 
                    //if (Input.GetMouseButtonDown(0) && !abilities[0].OnCooldown()) { //TODO make use of inputManager 
                    castTime = abilities[0].Trigger() + Network.time;
                    abilities[0].timer = (float)Network.time;
                }
                if (Input.GetKeyDown(KeyCode.E) && !abilities[1].OnCooldown()) { //TODO make use of inputManager 
                    castTime = abilities[1].Trigger() + Network.time;
                    abilities[1].timer = (float)Network.time;
                }
                if (Input.GetKeyDown(KeyCode.F) && !abilities[2].OnCooldown()) { //TODO make use of inputManager 
                    //if (Input.GetMouseButtonDown(1) && !abilities[2].OnCooldown()) { //TODO make use of inputManager 
                    //castTime = abilities[2].Trigger() + Network.time;
                    //abilities[2].timer = (float)Network.time;
                    abilities[2].Trigger();
                }
            } if (Input.GetMouseButton(0) && Input.GetMouseButton(1)) { vertAxis = 1; }
                #endregion
        }
        if (stats.isDead || castTime > Network.time || stats.isStunned || stats.isIncapacitated) { // Don't keep moving when dead~
            horizAxis = 0;
            vertAxis = 0;
            jump = false;
            //jumpAxis = 0;
        }
        if (transform.position.y < -100) { // Reset player position when they are below y-threshold
            transform.position = GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().GetSpawnPosition();
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            if (isLocalPlayer)
                stats.CmdTakeDmg(10000);
        }
    }

    /// <summary>
    /// Method in which the transform/player is actually moved
    /// </summary>
    public void Simulate() {
        //float speed;
        //SetSpeed(out speed);
        SetSpeed();
        // Movement
        transform.Translate(new Vector3(horizAxis, 0f, vertAxis) * Speed * Time.fixedDeltaTime);

        //Rotation
        if (Input.GetMouseButton(1) && (!stats.isDead && !stats.isStunned)) {// && !stats.isIncapacitated)) {// if dead they cannot turn their char around (but they can still look around with their camera)
            transform.rotation = Quaternion.Euler(0, cam.rotate.y, 0);
        }
        // Jumping
        //if(jumpAxis > 0 && isGrounded()) {
        if (jump && (isGrounded() || isSwimming)) {
            GetComponent<Rigidbody>().velocity = new Vector3(0, jumpSpeed, 0);
            dblJump = true;
            //Play 'Jump' Animation
            if (!isSwimming)//Shouldplay on Ground only.
                anim.SetTrigger("Jumped");
        }
        // Double Jumping 
        else if (jump && dblJump) {
            GetComponent<Rigidbody>().velocity += new Vector3(0, jumpSpeed, 0);
            dblJump = false;
            if (!isSwimming)//Shouldplay on Ground only.
                anim.SetTrigger("Jumped");
        }
        if (isNotFlying() && (anim.GetBool(Animator.StringToHash("IsFlying")) || anim.GetBool(Animator.StringToHash("IsFlyingFrontHands")))) {
            //Cancel the Flying Animation(s).
            if (isFlying || isFlyingFrontHands) {
                anim.SetBool("IsFlying", false);
                isFlying = false;
                anim.SetBool("IsFlyingFrontHands", false);
                isFlyingFrontHands = false;
            }
        }
        //transform.Translate(new Vector3(0f, jumpAxis, 0f) * jumpSpeed * Time.fixedDeltaTime);

        if (externalForce != new Vector3()) {
            GetComponent<Rigidbody>().velocity = externalForce;
            externalForce = new Vector3();
            CmdPushMe(new Vector3());
        }
    }

    public void PushMe(Vector3 force) {
        externalForce = force;
    }

    [Command]
    void CmdPushMe(Vector3 force) {
        externalForce = force;
    }

    bool isNotFlying() {
        //Raycast against Layers; Water, Ground, Trees.
        return Physics.Raycast(transform.position + (transform.localScale.y * .5f) * Vector3.up, -Vector3.up, GetComponent<Collider>().bounds.extents.y * 1.1f, (1 << 9) | (1 << 4) | (1 << 10));
    }

    bool isGrounded() {
        return Physics.Raycast(transform.position + (transform.localScale.y * .5f) * Vector3.up, -Vector3.up, GetComponent<Collider>().bounds.extents.y * 1.1f, ~(1 << 8));
    }
    //void OnCollisionEnter(Collision hit) { dblJump = true; }

    /// <summary>
    /// Sprint if shift is held
    /// </summary>
    /// <param name="speed">The speed at which the player will move</param>
    void SetSpeed(out float speed) {
        speed = stats ? stats.syncSpeed : Speed; // Only use speed from playerStats if it is not null
        //isWalking = !Input.GetKey(KeyCode.LeftShift);
        //speed = isWalking ? Speed : Speed * 2;
    }

    void SetSpeed() {
        Speed = stats ? stats.syncSpeed : Speed; // Only use speed from playerStats if it is not null
        if (stats.isSlowed)
            Speed *= .3f;
        //Play 'Walking' Animation
        anim.SetFloat("Speed", Speed * vertAxis);
    }

    public void StartSwimming() {
        if (!isSwimming) {
            if (Physics.Raycast(transform.position + (transform.localScale.y * .5f) * Vector3.up, -Vector3.up, drownDepth, ~(1 << 8))) { return; }
            isSwimming = true;
            //Play 'Swimming'Animation
            if (anim.GetBool(Animator.StringToHash("IsSwimming")) == false) {
                anim.SetBool("IsSwimming", true);
                //If we were flying we need to set the bools to false (so we don't fly in water! BWAH!).
                anim.SetBool("IsFlying", false);
                anim.SetBool("IsFlyingFrontHands", false);
            }
            StartCoroutine(InWater());
        }
    }

    IEnumerator InWater() {
        drownTimer = (float)Network.time;
        while (isSwimming && (((float)Network.time - drownTimer) < drownTime)) {
            if (GetComponent<PlayerStats>().isDead) { isSwimming = false; anim.SetBool("IsDeadByWater", true); anim.SetBool("IsSwimming", false); }
            if (Physics.Raycast(transform.position + (transform.localScale.y * .5f) * Vector3.up, -Vector3.up, drownDepth, ~(1 << 8))) {
                isSwimming = false;
                if (anim.GetBool(Animator.StringToHash("IsSwimming")))
                    anim.SetBool("IsSwimming", false);
                yield return null;
            }
            //print(drownTimeLeft());
            yield return new WaitForSeconds(1);
        } if (Physics.Raycast(transform.position + (transform.localScale.y * .5f) * Vector3.up, -Vector3.up, drownDepth, ~(1 << 8))) {
            isSwimming = false;
            if (anim.GetBool(Animator.StringToHash("IsSwimming")))
                anim.SetBool("IsSwimming", false);
            yield return null;
        }
        if (isLocalPlayer && isSwimming)
            stats.CmdTakeDmg(10000);
        isSwimming = false;
        if (anim.GetBool(Animator.StringToHash("IsSwimming")))
            anim.SetBool("IsSwimming", false);
        yield return null;
    }

    public float drownTimeLeft() {
        if (((float)Network.time - drownTimer) < drownTime)
            return ((float)Network.time - drownTimer) / drownTime;
        return 1;
    }
}
