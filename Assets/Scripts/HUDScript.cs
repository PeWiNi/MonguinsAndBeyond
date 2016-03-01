using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour {
    GameObject playerUI;
    GameObject actionBar;
    Image ability1;
    Image ability2;
    Image ability3;
    Slider healthSlider;
    Text healthText;
    PlayerStats ps;

    // Use this for initialization
    void Start () {
        playerUI = transform.FindChild("Player").gameObject;
        healthText = playerUI.GetComponentInChildren<Text>();
        healthSlider = playerUI.gameObject.GetComponentInChildren<Slider>();
        actionBar = transform.FindChild("ActionBar").gameObject;
        ability1 = actionBar.GetComponentsInChildren<Image>()[1]; // index 0 is the picture behind index 1
        ability2 = actionBar.GetComponentsInChildren<Image>()[3];
        ability3 = actionBar.GetComponentsInChildren<Image>()[5];
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
        }
    }

    /// <summary>
    /// Method for changing the fill of the overlay image corresponding to the ability
    /// </summary>
    /// <param name="overlayImage">Image for visualizing cooldown</param>
    /// <param name="ability">The ability corresponding to the action bar slot</param>
    public void ActionBarUpdate(ref Image overlayImage, Ability ability) {
        if (ability.OnCooldown() == true) {
            overlayImage.fillAmount = ability.CooldownRemaining();
        } 
    }

    /// <summary>
    /// Tie this HUD to a player's PlayerStats.cs script to visualize the correct player
    /// </summary>
    /// <param name="playerStats"></param>
    public void SetPlayerStats(PlayerStats playerStats) {
        ps = playerStats;
        ps.GetComponentInChildren<Canvas>().enabled = false;
        // TODO: Do stuff with setting up correct ability images
    }
}
