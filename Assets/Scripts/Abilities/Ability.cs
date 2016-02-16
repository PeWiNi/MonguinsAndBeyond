﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Ability : NetworkBehaviour {
    public float cooldown = 0.0f;
    [HideInInspector]
    public float timer = 0.0f;
    [SyncVar]
    int syncTeam;
    internal int team;
    public double castTime;

    // Use this for initialization
    void Start () {
        team = GetComponent<PlayerStats>().team;
    }
	
	// Update is called once per frame
	void Update () {
        SelectTeam();
    }

    void ApplyTeam() {
        if (!isLocalPlayer) {
            if (team != syncTeam) {
                team = syncTeam;
            }
        }
    }

    [ClientCallback]
    void SelectTeam() {
        if (isLocalPlayer) {
            CmdProvideTeam(team);
        }
    }

    [Command]
    void CmdProvideTeam(int team) {
        syncTeam = team;
    }

    public virtual double Trigger() { return castTime; }

    [Command]
    internal void CmdDamagePlayer(GameObject player, float damage) {
        player.GetComponent<PlayerStats>().TakeDmg(damage);
    }
    [Command]
    internal void CmdStunPlayer(GameObject player, float duration) {
        player.GetComponent<PlayerStats>().Stun(duration);
    }
}