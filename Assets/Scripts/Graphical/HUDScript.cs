using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour {
    GameObject playerUI;
    public Inventory inventory;
    Slider castBar;
    GameObject actionBar;
    Image ability1;
    Image ability2;
    Image ability3;
    Slider healthSlider;
    Text healthText;
    PlayerStats ps;
    PlayerLogic pl;

    #region Trap action bar fields
    Image trap1;
    public float trap1Cooldown = 10f;
    float trap1Timer;
    Image trap2;
    public float trap2Cooldown = 25f;
    float trap2Timer;
    Image trap3;
    public float trap3Cooldown = 10f;
    float trap3Timer;
    #endregion

    double gameTimer;

    CursorLockMode wantedMode;
    // Cast bar reference to ensure the correct bar is showing
    string currentText = "";

    // Use this for initialization
    void Start () {
        playerUI = transform.FindChild("Player").gameObject;
        healthText = playerUI.GetComponentInChildren<Text>();
        healthSlider = playerUI.gameObject.GetComponentInChildren<Slider>();
        actionBar = transform.FindChild("ActionBar").gameObject;
        ability1 = actionBar.GetComponentsInChildren<Image>()[1]; // index 0 is the picture behind index 1
        ability2 = actionBar.GetComponentsInChildren<Image>()[3];
        ability3 = actionBar.GetComponentsInChildren<Image>()[5];

        trap1 = actionBar.GetComponentsInChildren<Image>()[7];
        trap2 = actionBar.GetComponentsInChildren<Image>()[9];
        inventory = transform.FindChild("Inventory").GetComponent<Inventory>();
        castBar = transform.FindChild("CastBar").GetComponent<Slider>();
        castBar.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if(ps != null) {
            #region Health Bar
            healthText.text = (int)System.Math.Ceiling(ps.health) + "/" + System.Math.Ceiling(ps.maxHealth);
            healthSlider.value = (ps.health / ps.maxHealth);
            #endregion
            #region Action Bar
            ActionBarUpdate(ref ability1, ps.abilities[0]);
            ActionBarUpdate(ref ability2, ps.abilities[1]);
            ActionBarUpdate(ref ability3, ps.abilities[2]);
            #endregion
            #region Trap Bar
            if (Input.GetKeyDown(KeyCode.Alpha1) && !ps.GetComponentInChildren<SpawnTraps>().isPlacing && !OnCooldown(trap1Cooldown, trap1Timer) && ps.CanIMove()) {
                SpawnBananaTrap();
            }
            ActionBarUpdate(ref trap1, trap1Cooldown, trap1Timer);

            if (Input.GetKeyDown(KeyCode.Alpha2) && !ps.GetComponentInChildren<SpawnTraps>().isPlacing && !OnCooldown(trap2Cooldown, trap2Timer) && ps.CanIMove()) {
                SpawnSpikeTrap();
            }
            ActionBarUpdate(ref trap2, trap2Cooldown, trap2Timer);

            if (Input.GetKeyDown(KeyCode.Alpha3) && !ps.GetComponentInChildren<SpawnTraps>().isPlacing && !OnCooldown(trap3Cooldown, trap3Timer) && ps.CanIMove()) {
                SpawnSapTrap();
            }
            ActionBarUpdate(ref trap2, trap2Cooldown, trap2Timer);
            #endregion
            #region Cast Bar (currently working for drowning and respawning)
            if (ps.isDead) { // RESPAWNING
                if (!castBar.gameObject.activeSelf || currentText != "Respawning") { //Reset graphics and text if disabled or using the wrong text
                    castBar.gameObject.SetActive(true);
                    castBar.fillRect.GetComponentInChildren<Image>().color = new Color(238f / 255f, 0f, 2f / 255f);
                    castBar.targetGraphic.GetComponentInChildren<Image>().color = new Color(51f / 255f, 68f / 255f, 34f / 255f);
                    currentText = "Respawning";
                    castBar.GetComponentInChildren<Text>().text = currentText;
                }
                float value = 1 - (ps.deathTimeLeft() / ps.deathCooldown);
                if (value > 0) 
                    castBar.value = value;
                else
                    castBar.gameObject.SetActive(false);
            } else if (ps.isStunned) { // STUNNED
                if (!castBar.gameObject.activeSelf || currentText.Substring(0, 7) != "Stunned") {
                    castBar.gameObject.SetActive(true);
                    castBar.fillRect.GetComponentInChildren<Image>().color = new Color(248f / 255f, 74f / 255f, 2f / 255f);
                    castBar.targetGraphic.GetComponentInChildren<Image>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
                    currentText = "Stunned";
                    castBar.GetComponentInChildren<Text>().text = currentText;
                    castBar.value = 1;
                }
                float value = ps.stunTimeLeft();
                if (value > 0.01f)
                    castBar.GetComponentInChildren<Text>().text = "Stunned for " + Mathf.Ceil(value) + "s..";
                else
                    castBar.gameObject.SetActive(false);
            } else if(pl.isSwimming) { // DROWNING
                if (!castBar.gameObject.activeSelf || currentText != "Drowning") {
                    castBar.gameObject.SetActive(true);
                    castBar.fillRect.GetComponentInChildren<Image>().color = new Color(67f / 255f, 112f / 255f, 238f / 255f);
                    castBar.targetGraphic.GetComponentInChildren<Image>().color = new Color(51f / 255f, 68f / 255f, 255f / 255f);
                    currentText = "Drowning";
                    castBar.GetComponentInChildren<Text>().text = currentText;
                }
                float value = 1 - pl.drownTimeLeft();
                if (value > 0.01f)
                    castBar.value = value;
                else
                    castBar.gameObject.SetActive(false);
            }  else if (castBar.gameObject.activeSelf) {
                castBar.gameObject.SetActive(false);
            }
        }
        #endregion
        // When pressing Alt the mouse will be released from whatever state is set
        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            if (wantedMode != CursorLockMode.None) {
                Cursor.lockState = wantedMode = CursorLockMode.None;
                SetCursorState();
            } else
                SetCursorState(true);
        }
    }

    #region Action Bar methods
    /// <summary>
    /// Method for changing the fill of the overlay image corresponding to the ability
    /// </summary>
    /// <param name="overlayImage">Image for visualizing cooldown</param>
    /// <param name="ability">The ability corresponding to the action bar slot</param>
    public void ActionBarUpdate(ref Image overlayImage, Ability ability) {
        if (ability.OnCooldown()) {
            overlayImage.fillAmount = ability.CooldownRemaining();
        } else if (overlayImage.fillAmount < 1) {
            overlayImage.fillAmount = 1;
        }
    }

    /// <summary>
    /// Extended method for when not using Abilities
    /// Useful for giving the trap icons the same functionality as the abilities
    /// </summary>
    /// <param name="overlayImage">Image for visualizing cooldown</param>
    /// <param name="cooldown">The time until the ability can be triggered again</param>
    /// <param name="time">Timer of the ability</param>
    public void ActionBarUpdate(ref Image overlayImage, float cooldown, float time) {
        if (OnCooldown(cooldown, time))
            overlayImage.fillAmount = (1.0f / cooldown * (Time.time - time));
        else if (overlayImage.fillAmount < 1) 
            overlayImage.fillAmount = 1;
    }

    bool OnCooldown(float cooldown, float timer) {
        return (Time.time - timer < cooldown);
    }
    #endregion

    /// <summary>
    /// Tie this HUD to a player's PlayerStats.cs script to visualize the correct player
    /// </summary>
    /// <param name="playerStats"></param>
    public void SetPlayerStats(PlayerStats playerStats) {
        ps = playerStats;
        pl = playerStats.GetComponent<PlayerLogic>();
        // Turn off world-space healthBar
        ps.GetComponentInChildren<Canvas>().enabled = false;
        #region Inventory
        //Activate Inventory GO
        inventory.gameObject.SetActive(true);
        //Set count status text
        inventory.transform.FindChild("Banana").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().bananaCount;
        inventory.transform.FindChild("Stick").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().stickCount;
        inventory.transform.FindChild("Sap").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().stickCount;
        inventory.transform.FindChild("Leaf").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().stickCount;
        inventory.transform.FindChild("BerryR").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().berryRCount;
        inventory.transform.FindChild("BerryG").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().berryGCount;
        inventory.transform.FindChild("BerryB").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().berryBCount;
        #endregion
        // TODO: Do stuff with setting up correct ability images
        SetCursorState(true);
    }

    public PlayerStats GetPlayerStats() {
        return ps;
    }

    #region Cursor state methods
    /// <summary>
    /// Allow the current camera controls to determine the mouse lock state
    /// Also possible to simply apply LockState (when false)
    /// </summary>
    /// <param name="useStates">Whether or not to let the controls determine the state</param>
    public void SetCursorState(bool useStates = false) {
        if (useStates) {
            //wantedMode = CursorLockMode.Confined;
            wantedMode = CursorLockMode.None;
        }
        Cursor.lockState = wantedMode;
        // Hide cursor when locking
        Cursor.visible = (CursorLockMode.Locked != wantedMode);
    }

    /// <summary>
    /// Set a specific LockMode state
    /// </summary>
    /// <param name="lockMode">The requested state</param>
    public void SetCursorState(CursorLockMode lockMode) {
        wantedMode = lockMode;
        SetCursorState();
    }
    #endregion

    #region Environment methods
    public void SpawnBananaTrap() {
        if (inventory.useBanana()) 
            StartCoroutine(PlaceBananaTrap());
        inventory.transform.FindChild("Banana").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().bananaCount;
    }

    public void SpawnSpikeTrap() {
        if (inventory.useForSpikes())
            StartCoroutine(PlaceSpikeTrap());
        inventory.transform.FindChild("Stick").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().stickCount;
        inventory.transform.FindChild("Leaf").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().leafCount;
    }

    public void SpawnSapTrap() {
        if (inventory.useBanana())
            StartCoroutine(PlaceSapTrap());
        inventory.transform.FindChild("Sap").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().sapCount;
    }

    IEnumerator PlaceBananaTrap() {
        SpawnTraps waitFor = ps.GetComponentInChildren<SpawnTraps>();
        int bananaCount = inventory.bananaCount;
        yield return StartCoroutine(waitFor.Slippery());
        // If the counter is unchanged, this means that the coroutine has not refunded the banana
        if(bananaCount == inventory.bananaCount) // Not entirely correct to not start timer; this is due to the player being able to pick up bananas while placing
            trap1Timer = Time.time;
        yield return null;
    }

    IEnumerator PlaceSpikeTrap() {
        SpawnTraps waitFor = ps.GetComponentInChildren<SpawnTraps>();
        int stickCount = inventory.stickCount;
        yield return StartCoroutine(waitFor.Spikey());
        if (stickCount == inventory.stickCount) // Rogue Codez!
            trap2Timer = Time.time;
        yield return null;
    }

    IEnumerator PlaceSapTrap() {
        SpawnTraps waitFor = ps.GetComponentInChildren<SpawnTraps>();
        int sapCount = inventory.sapCount;
        yield return StartCoroutine(ps.GetComponentInChildren<SpawnTraps>().StickySap());
        if (sapCount == inventory.sapCount) // Rogue Codez!
            trap3Timer = Time.time;
        yield return null;
    }

    /// <summary>
    /// Drop an item in the world from the Inventory
    /// Only possible if the player has at least 1 of the item
    /// </summary>
    /// <param name="item">String representation of the item in question</param>
    public void DropItem(string item) {
        if (ps.CanIMove()) {
            //Drop items if holding Shift
            if (Input.GetKey(KeyCode.LeftShift) && (
                item == "Banana" ? inventory.useBanana() :
                item == "Stick" ? inventory.useSticks() :
                item == "Sap" ? inventory.useSap() :
                item == "Leaf" ? inventory.useLeaf() :
                item.Substring(0, 5) == "Berry" ? inventory.useBerry(item) : false))
                ps.GetComponent<SyncInventory>().CmdSpawnItem(item, new Vector3(), 0);

            // Banana Trap
            else if (!OnCooldown(trap1Cooldown, trap1Timer) && item == "Banana" ? inventory.useBanana() : false)
                StartCoroutine(PlaceBananaTrap());

            // Spike Trap
            else if (!OnCooldown(trap2Cooldown, trap2Timer) && item == "Stick" ? inventory.useForSpikes() : false) {
                StartCoroutine(PlaceSpikeTrap());
                inventory.transform.FindChild("Leaf").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().leafCount;
            }

            //Throw Sap
            else if (!OnCooldown(trap3Cooldown, trap3Timer) && item == "Sap" ? inventory.useSap() : false)
                StartCoroutine(PlaceSapTrap());

            // Camouflage
            else if (item == "Leaf" ? inventory.useLeaf() : false)
                ps.GetComponent<SyncInventory>().CmdUseLeaf();


            else if (item.Substring(0, 5) == "Berry" ? inventory.useBerry(item) : false) {
                ps.EatBerry(item);
                //Herb berry = new Herb();
                //berry.ChangeProperties(item, ps.team);
                //berry.EatIt(ps);
            }

            // Update text field with new values
            inventory.transform.FindChild(item).GetComponentInChildren<Text>().text =
                item == "Banana" ? "" + inventory.GetComponent<Inventory>().bananaCount :
                item == "Stick" ? "" + inventory.GetComponent<Inventory>().stickCount :
                item == "Sap" ? "" + inventory.GetComponent<Inventory>().sapCount :
                item == "Leaf" ? "" + inventory.GetComponent<Inventory>().leafCount :
                item == "BerryR" ? "" + inventory.GetComponent<Inventory>().berryRCount :
                item == "BerryG" ? "" + inventory.GetComponent<Inventory>().berryGCount :
                item == "BerryB" ? "" + inventory.GetComponent<Inventory>().berryBCount :
                "";
        }
    }
    #endregion

    void OnEnable() {
        EventManager.EventScoreChange += UpdateDeathScore;
    }

    // Just to make sure that we unsubscribe when the object is no longer in use
    void OnDisable() {
        EventManager.EventScoreChange -= UpdateDeathScore;
    }

    /// <summary>
    /// Method in charge of updating the score
    /// </summary>
    /// <param name="team1">Death count for Team 1</param>
    /// <param name="team2">Death count for Team 2</param>
    void UpdateDeathScore(float team1, float team2) {
        Text[] textiez = transform.FindChild("ScoreBoard").GetComponentsInChildren<Text>();
        textiez[1].text = "Team 1: " + team1 + " deaths";
        textiez[2].text = "Team 2: " + team2 + " deaths";
    }

    public void SetupTimer(double time) {
        //gameTimer = time;
        StartCoroutine(LikeClockWork(time));
    }

    IEnumerator LikeClockWork(double time) {
        while(true) {
            yield return new WaitForSeconds(1);
            float timez = (float)(Network.time - time);
            float minute = Mathf.Floor(timez / 60);
            float seconds = Mathf.Floor(timez % 60);
            transform.FindChild("ScoreBoard").FindChild("Timer").GetComponent<Text>().text = string.Format("{0:00}:{1:00}", minute, seconds);
        }
    }
}
