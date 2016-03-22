using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour {
    GameObject playerUI;
    public Inventory inventory;
    Toggle CamMouse;
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
    #endregion

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
        CamMouse = transform.FindChild("Toggle CamMouse").GetComponent<Toggle>();
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
            if (Input.GetKeyDown(KeyCode.Alpha1) && !ps.GetComponentInChildren<SpawnTraps>().isPlacing && !OnCooldown(trap1Cooldown, trap1Timer)) {
                SpawnBananaTrap();
            }
            ActionBarUpdate(ref trap1, trap1Cooldown, trap1Timer);

            if (Input.GetKeyDown(KeyCode.Alpha2) && !ps.GetComponentInChildren<SpawnTraps>().isPlacing && !OnCooldown(trap2Cooldown, trap2Timer)) {
                SpawnSpikeTrap();
            }
            ActionBarUpdate(ref trap2, trap2Cooldown, trap2Timer);
            #endregion
            if(ps.isDead) {
                if (!castBar.gameObject.activeSelf) {
                    castBar.gameObject.SetActive(true);
                    castBar.fillRect.GetComponentInChildren<Image>().color = new Color(238f / 255f, 0f, 2f / 255f);
                    castBar.targetGraphic.GetComponentInChildren<Image>().color = new Color(51f / 255f, 68f / 255f, 34f / 255f);
                    castBar.GetComponentInChildren<Text>().text = "Respawning";
                }
                float value = 1 - (ps.deathTimeLeft() / ps.deathCooldown);
                if (value > 0) 
                    castBar.value = value;
                else
                    castBar.gameObject.SetActive(false);
            } else if(pl.isSwimming) {
                if (!castBar.gameObject.activeSelf) {
                    castBar.gameObject.SetActive(true);
                    castBar.fillRect.GetComponentInChildren<Image>().color = new Color(67f / 255f, 112f / 255f, 238f / 255f);
                    castBar.targetGraphic.GetComponentInChildren<Image>().color = new Color(51f / 255f, 68f / 255f, 255f / 255f);
                    castBar.GetComponentInChildren<Text>().text = "Drowning";
                }
                float value = 1 - pl.drownTimeLeft();
                if (value > 0)
                    castBar.value = value;
                else
                    castBar.gameObject.SetActive(false);
            } else if (castBar.gameObject.activeSelf) {
                castBar.gameObject.SetActive(false);
            }
        }
    }

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

    public void ActionBarUpdate(ref Image overlayImage, float cooldown, float time) {
        if (OnCooldown(cooldown, time))
            overlayImage.fillAmount = (1.0f / cooldown * (Time.time - time));
        else if (overlayImage.fillAmount < 1) 
            overlayImage.fillAmount = 1;
    }

    bool OnCooldown(float cooldown, float timer) {
        return (Time.time - timer < cooldown);
    }

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
        inventory.transform.FindChild("Sticks").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().stickCount;
        inventory.transform.FindChild("BerryR").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().berryRCount;
        inventory.transform.FindChild("BerryG").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().berryGCount;
        inventory.transform.FindChild("BerryB").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().berryBCount;
        #endregion
        ps.GetComponent<PlayerLogic>().SetCameraControl(CamMouse.isOn);
        // TODO: Do stuff with setting up correct ability images
    }

    public void eventValueChanged() {
        if(ps != null)
            ps.GetComponent<PlayerLogic>().SetCameraControl(CamMouse.isOn);
        Debug.Log("Toggle is " + CamMouse.isOn); //check isOn state
    }

    public PlayerStats GetPlayerStats() {
        return ps;
    }

    public void SpawnBananaTrap() {
        if (inventory.useBanana()) 
            StartCoroutine(PlaceBananaTrap());
        inventory.transform.FindChild("Banana").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().bananaCount;
    }

    public void SpawnSpikeTrap() {
        if (inventory.useSticks())
            StartCoroutine(PlaceSpikeTrap());
        inventory.transform.FindChild("Stick").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().stickCount;
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

    public void DropItem(string item) {
        if(item == "Sticks" ? inventory.useSticks() : item == "Banana" ? inventory.useBanana() : item.Substring(0, 5) == "Berry" ? inventory.useBerry(item) : false)
            ps.GetComponent<SyncInventory>().CmdSpawnItem(item, new Vector3(), 0);
        inventory.transform.FindChild(item).GetComponentInChildren<Text>().text = 
            item == "Sticks" ? "" + inventory.GetComponent<Inventory>().stickCount :
            item == "Banana" ? "" + inventory.GetComponent<Inventory>().bananaCount :
            item == "BerryR" ? "" + inventory.GetComponent<Inventory>().berryRCount :
            item == "BerryG" ? "" + inventory.GetComponent<Inventory>().berryGCount :
            item == "BerryB" ? "" + inventory.GetComponent<Inventory>().berryBCount :
            "";
    }

    void OnEnable() {
        EventManager.EventScoreChange += UpdateColor;
    }


    void OnDisable() {
        EventManager.EventScoreChange -= UpdateColor;
    }

    void UpdateColor(float team1, float team2) {
        Text[] textiez = transform.FindChild("ScoreBoard").GetComponentsInChildren<Text>();
        textiez[1].text = "Team 1: " + team1 + " deaths";
        textiez[2].text = "Team 2: " + team2 + " deaths";
    }
}
