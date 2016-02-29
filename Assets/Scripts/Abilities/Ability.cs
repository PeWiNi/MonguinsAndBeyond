﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// High-level class for player abilities containing basic functions
/// Trigger() is overridden in order to issue the actual ability functionality issued in the PlayerLogic.cs
/// </summary>
public class Ability : NetworkBehaviour {
    public float cooldown = 0.0f;
    [HideInInspector]
    public float timer = 0.0f;
    internal int team;
    public double castTime;

    // Use this for initialization
    void Start () { }
	
	// Update is called once per frame
	void Update () {
        SelectTeam();
    }

    [ClientCallback]
    void SelectTeam() { // mayhaps send as command? idk, maybe I will care in the future
        if (isLocalPlayer) {
            team = GetComponent<PlayerStats>().team;
        }
    }

    /// <summary>
    /// Method issued when using the ability
    /// </summary>
    /// <returns>The time in which the player is unable to move while casting the ability</returns>
    public virtual double Trigger() { return castTime; }

    /// <summary>
    /// Method for dealing damage directly to players
    /// </summary>
    /// <param name="player">Players on the recieving end of the damage</param>
    /// <param name="damage">The amount of damage dealt</param>
    [Command]
    internal void CmdDamagePlayer(GameObject player, float damage) {
        player.GetComponent<PlayerStats>().TakeDmg(damage);
    }
    /// <summary>
    /// Method for stunning players
    /// --Stunning incapacitates players/users, disabling their ability to control their character--
    /// </summary>
    /// <param name="player">Players on the recieving end of the stun</param>
    /// <param name="duration">For how long will they be stunned?</param>
    [Command]
    internal void CmdStunPlayer(GameObject player, float duration) {
        player.GetComponent<PlayerStats>().Stun(duration);
    }
}
