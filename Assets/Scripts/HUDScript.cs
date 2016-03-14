using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour {
    GameObject playerUI;
    public Inventory inventory;
    GameObject actionBar;
    Image ability1;
    Image ability2;
    Image ability3;
    Slider healthSlider;
    Text healthText;
    PlayerStats ps;
    Image trap1;
    public float trap1Cooldown = 10f;
    float trap1Timer;

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
        inventory = transform.FindChild("Inventory").GetComponent<Inventory>();
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
            #endregion
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
        // Turn off world-space healthBar
        ps.GetComponentInChildren<Canvas>().enabled = false;
        #region Inventory
        //Activate Inventory GO
        inventory.gameObject.SetActive(true);
        //Set Banana-count text
        inventory.transform.FindChild("Banana").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().bananaCount;
        #endregion

        // TODO: Do stuff with setting up correct ability images
    }

    public void SpawnBananaTrap() {
        if (inventory.useBanana()) 
            StartCoroutine(PlaceBananaTrap());
        inventory.transform.FindChild("Banana").GetComponentInChildren<Text>().text = "" + inventory.GetComponent<Inventory>().bananaCount;
    }

    IEnumerator PlaceBananaTrap() {
        SpawnTraps waitFor = ps.GetComponentInChildren<SpawnTraps>();
        int bananaCount = inventory.bananaCount;
        yield return StartCoroutine(waitFor.Slippery());
        if(bananaCount == inventory.bananaCount) // Rogue codez!
            trap1Timer = Time.time;
        yield return null;
    }
}
