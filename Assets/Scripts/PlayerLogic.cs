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

    NetworkAnimator animNetwork;
    Animator animLocal;
    PlayerBehaviour playerBehaviour;
    public bool isFlying;
    public bool isFlyingFrontHands;

    public PlayerMovementSync NetworkMovement {
        get {
            throw new System.NotImplementedException();
        }

        set {
        }
    }

    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();
        int countParameters = GetComponent<NetworkAnimator>().animator.parameterCount;
        print("Count Parameters Start Local Player = " + countParameters);
        for (int i = 0; i < countParameters; i++) {
            GetComponent<NetworkAnimator>().SetParameterAutoSend(i, true);
        }
    }

    public override void PreStartClient() {
        int countParameters = GetComponent<NetworkAnimator>().animator.parameterCount;
        print("Count Parameters Pre Start Client = " + countParameters);
        for (int i = 0; i < countParameters; i++) {
            GetComponent<NetworkAnimator>().SetParameterAutoSend(i, true);
        }
    }

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
        animNetwork = GetComponent<NetworkAnimator>();
        animLocal = GetComponent<Animator>();
        playerBehaviour = GetComponent<PlayerBehaviour>();
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
                    castTime = abilities[0].Trigger() + Network.time;
                    //abilities[0].timer = (float)Network.time;
                    playerBehaviour.isIdle = false;
                }
                if (Input.GetKeyDown(KeyCode.E) && !abilities[1].OnCooldown()) { //TODO make use of inputManager 
                    castTime = abilities[1].Trigger() + Network.time;
                    //abilities[1].timer = (float)Network.time;
                    playerBehaviour.isIdle = false;
                }
                if (Input.GetKeyDown(KeyCode.F) && !abilities[2].OnCooldown()) { //TODO make use of inputManager 
                    //castTime = abilities[2].Trigger() + Network.time;
                    //abilities[2].timer = (float)Network.time;
                    abilities[2].Trigger();
                    playerBehaviour.isIdle = false;
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
        if (jump && isGrounded()) {
            GetComponent<Rigidbody>().velocity = new Vector3(0, jumpSpeed, 0);
            dblJump = true;
            //Play 'Jump' Animation
            animNetwork.SetTrigger("Jumped");
        }
        // Double Jumping 
        else if (jump && dblJump) {
            GetComponent<Rigidbody>().velocity += new Vector3(0, jumpSpeed, 0);
            dblJump = false;
            //Play 'Jump' Animation
            if (!isSwimming)//Shouldplay on Ground only.
                animNetwork.SetTrigger("Jumped");
        }
        if (isNotFlying() && (animLocal.GetBool(Animator.StringToHash("IsFlying")) || animLocal.GetBool(Animator.StringToHash("IsFlyingFrontHands")))) {
            //Cancel the Flying Animation(s).
            if (isFlying || isFlyingFrontHands) {
                animLocal.SetBool("IsFlying", false);
                isFlying = false;
                animLocal.SetBool("IsFlyingFrontHands", false);
                isFlyingFrontHands = false;
            }
        }
        //transform.Translate(new Vector3(0f, jumpAxis, 0f) * jumpSpeed * Time.fixedDeltaTime);

        if (externalForce != new Vector3()) {
            GetComponent<Rigidbody>().velocity = externalForce;
            externalForce = new Vector3();
            CmdPushMe(new Vector3());
        }
        if (stats.isTaunted() != null) {
            Quaternion before = transform.rotation;
            transform.LookAt(stats.isTaunted());
            Quaternion after = transform.rotation;
            float rot = 5 * Time.deltaTime; // Adjust speed?
            transform.rotation = Quaternion.Lerp(before, after, rot);
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
        animLocal.SetFloat("Speed", Speed * vertAxis);
    }

    #region Swim stuff
    public void StartSwimming() {
        if (!isSwimming) {
            if (Physics.Raycast(transform.position + (transform.localScale.y * .5f) * Vector3.up, -Vector3.up, drownDepth, ~(1 << 8))) { return; }
            isSwimming = true;
            //Play 'Swimming'Animation
            if (animLocal.GetBool(Animator.StringToHash("IsSwimming")) == false) {
                animLocal.SetBool("IsSwimming", true);
                //If we were flying we need to set the bools to false (so we don't fly in water! BWAH!).
                animLocal.SetBool("IsFlying", false);
                animLocal.SetBool("IsFlyingFrontHands", false);
            }
            StartCoroutine(InWater());
        }
        dblJump = true;
    }

    IEnumerator InWater() {
        drownTimer = (float)Network.time;
        while (isSwimming && (((float)Network.time - drownTimer) < drownTime)) {
            if (GetComponent<PlayerStats>().isDead) { isSwimming = false; animLocal.SetBool("IsDeadByWater", true); animLocal.SetBool("IsSwimming", false); }
            if (Physics.Raycast(transform.position + (transform.localScale.y * .5f) * Vector3.up, -Vector3.up, drownDepth, ~(1 << 8))) {
                isSwimming = false;
                if (animLocal.GetBool(Animator.StringToHash("IsSwimming")))
                    animLocal.SetBool("IsSwimming", false);
                yield return null;
            }
            //print(drownTimeLeft());
            yield return new WaitForSeconds(1);
        } if (Physics.Raycast(transform.position + (transform.localScale.y * .5f) * Vector3.up, -Vector3.up, drownDepth, ~(1 << 8))) {
            isSwimming = false;
            if (animLocal.GetBool(Animator.StringToHash("IsSwimming")))
                animLocal.SetBool("IsSwimming", false);
            yield return null;
        }
        if (isLocalPlayer && isSwimming)
            stats.CmdTakeDmg(10000);
        isSwimming = false;
        if (animLocal.GetBool(Animator.StringToHash("IsSwimming")))
            animLocal.SetBool("IsSwimming", false);
        yield return null;
    }

    public float drownTimeLeft() {
        if (((float)Network.time - drownTimer) < drownTime)
            return ((float)Network.time - drownTimer) / drownTime;
        return 1;
    }
    #endregion
    #region HUD Score stuff
    void OnEnable() {
        GetComponent<EventManager>().EventScoreChange += UpdateScoreInHUD;
        GetComponent<EventManager>().EventScoreBoard += ScoreBoardInHUD;
    }

    // Just to make sure that we unsubscribe when the object is no longer in use
    void OnDisable() {
        GetComponent<EventManager>().EventScoreChange -= UpdateScoreInHUD;
        GetComponent<EventManager>().EventScoreBoard -= ScoreBoardInHUD;
    }

    void UpdateScoreInHUD(float team1, float team2) { // Should be isLocalPlayer but #cba it finally works
        GameObject.Find("HUD").GetComponent<HUDScript>().UpdateDeathScore(team1, team2);
    }

    void ScoreBoardInHUD(string[] names, int[] team, int[] kills, int[] deaths, float[] score, int[] teamKills, int[] teamDeaths, float[] teamScore) {
        GameObject.Find("HUD").GetComponent<HUDScript>().SetupScoreBoard(names, team, kills, deaths, score, teamKills, teamDeaths, teamScore);
    }

    public void TriggerScoreBoard() {
        CmdTriggerScoreBoard();
    }

    [Command]
    void CmdTriggerScoreBoard() {
        ScoreManager SM = GameObject.Find("NetworkManager").GetComponentInChildren<ScoreManager>();
        PlayerStats[] ps = SM.GetScoreBoard();
        string[] names = new string[ps.Length];
        int[] team = new int[ps.Length];
        int[] kills = new int[ps.Length];
        int[] deaths = new int[ps.Length];
        float[] score = new float[ps.Length];
        int[] teamKills = new int[2] { SM.teamOneKillCount, SM.teamTwoKillCount };
        int[] teamDeaths = new int[2] { SM.teamOneDeathCount, SM.teamTwoDeathCount };
        float[] teamScore = new float[2] { SM.teamOneScore, SM.teamTwoScore };
        for (int i = 0; i < ps.Length; i++) {
            if (!ps[i]) continue;
            names[i] = ps[i].playerName;
            team[i] = ps[i].team;
            kills[i] = ps[i].kills;
            deaths[i] = ps[i].deaths;
            score[i] = ps[i].score;
        }
        GetComponent<EventManager>().SendScoreBoardEvent(names, team, kills, deaths, score, teamKills, teamDeaths, teamScore);
    }
    #endregion
}
