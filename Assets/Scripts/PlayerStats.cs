﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerStats : NetworkBehaviour {

    [SyncVar(hook = "SyncMaxHealth")]
    float syncMaxHealth = 1000f;
    [SyncVar]
    float syncHealth;
    [SyncVar]
    int resilience; // Recieved damage modifier, 100 means 100% dmg reduction
    [SyncVar]
    Role syncRole = Role.Basic;

    #region Death
    [SyncVar]
    public bool isDead = false;
    [SyncVar]
    float deathTimer;
    public float deathCooldown = 15f;
    #endregion

    [SerializeField]
    public float maxHealth; // Full amount of health
    [SerializeField]
    public float health; // Reach 0 and you die
    [SerializeField]
    private Hashtable attributes; // Strength, Agility, Wisdom, etc. and their respective values // SYNCVAR?
    [SerializeField]
    Role role = Role.Basic; // Current primary Role to determine abilities
    [Range(0.2f, 2.5f)] [SyncVar]
    public float sizeModifier = 1f;
    [Range(0.5f, 10f)] [SyncVar]
    public float speed = 5f; // Movement (and jumping) speed (see PlayerLogic.cs)
    [SerializeField]
    public Transform body;
    [SyncVar]
    public int team;
    [SyncVar]
    public bool changeMaxHealth = false;

    public Ability[] abilities;

    RoleStats roleStats;

    [SyncVar]
    public bool isStunned = false;
    [SyncVar]
    double stunTimer;

    [SyncVar]
    bool makeMap = false;

    public GameObject bulletPrefab;
    
    public enum Role {
        Basic, Defender, Attacker, Supporter
    }

    public struct RoleStats {
        public float maxHealth;
        public int resilience;
        public float speed;

        public RoleStats(float mHp, int resi, float spd) {
            maxHealth = mHp;
            resilience = resi;
            speed = spd;
        }
    }

    Role determineRole(string s) {
        switch(s) {
            case "DEF":
                return Role.Defender;
            case "ATT":
                return Role.Attacker;
            case "SUP":
                return Role.Supporter;
            default:
                return Role.Basic;
        }
    }

	// Use this for initialization
	void Start () {
        if (isLocalPlayer) {
            #region Loading attributes and determining Role
            // Load Attributes set before entering game
            MyNetworkManagerHUD NM = GameObject.Find("NetworkManager").GetComponent<MyNetworkManagerHUD>();
            attributes = NM.getAttributes();
            /*
            // Check for highest value
            DictionaryEntry max = new DictionaryEntry("s", 0);
            foreach (DictionaryEntry de in attributes) {
                if ((int)de.Value > (int)max.Value)
                    max = de;
            }
            //Determine primary role (if near middle he is just set to Basic)
            if ((int)max.Value > (100 / attributes.Count) + 5) {
                role = determineRole((string)max.Key);
            } else role = Role.Basic;
            */ //Kept in case other roles are wanted
            role = Role.Attacker;
            #endregion
            //CmdTeamSelection(NM.team);
            CmdTeamSelection(NM.team > 0 ? NM.team : team);
            RoleCharacteristics(role);
            SelectRole();
            try { GameObject.Find("HUD").GetComponent<HUDScript>().SetPlayerStats(this); } catch { }
        }
        syncHealth = syncMaxHealth;
    }
	
	// Update is called once per frame
	void Update () {
        if (isDead) { // Send to co-routine?
            Color c = body.GetComponent<MeshRenderer>().material.color;
            if (((float)Network.time - deathTimer) > deathCooldown) {
                foreach (Material m in body.GetComponent<MeshRenderer>().materials)
                    m.color = new Color(c.r, c.g, c.b, 1f);
                if (isLocalPlayer) {
                    Respawn();
                    CmdRespawn();
                }
                return;
            }
            health = 0;

            foreach (Material m in body.GetComponent<MeshRenderer>().materials)m.color = new Color(c.r, c.g, c.b, .2f);
            return;
        } if (isStunned) {
            if (stunTimer < Network.time) {
                isStunned = false;
            }
        }
        TeamSelect();
        ApplyRole();
        StatSync();

        if(changeMaxHealth) { NewPlayerJoinedTeam(); }
        if(makeMap) { GenerateTerrain(); }
    }

    void ApplyRole() {
        if (!isLocalPlayer) {
            if (role != syncRole) {
                role = syncRole;
                RoleCharacteristics(syncRole);
            }
        }
    }
    /// <summary>
    /// Apply characteristics of Role
    /// Taken from Wikia, last updated: 24th of November 2015
    /// </summary>
    /// <param name="role">The player Role</param>
    void RoleCharacteristics(Role role) {
        abilities = new Ability[3];
        switch (role) {
            case (Role.Defender):
                roleStats = new RoleStats(1.3f, (int)Mathf.Ceil(0.2f * (int)attributes["DEF"]), 0.80f);
                syncMaxHealth *= 1.3f;
                if (isLocalPlayer && attributes.ContainsKey("DEF")) //increased resilience based on STR
                    resilience = (int)Mathf.Ceil(0.2f * (int)attributes["DEF"]);
                //  Taunt (Roar/Growl/WTV) - taunts enemies (locks their target on him for 3 sec) CD:6 sec
                //  Smash (deals 1% of enemy health and stuns 1 sec) - no CD, should take 1sec to fully cast anyway
                //  Fortify - temporarily increase health and resilience of the defender with 20% for 10 sec. CD:20sec
                speed *= 0.80f;
                // Placeholder visual thing
                body.GetComponent<MeshRenderer>().material.color = Color.blue;
                break;
            case (Role.Attacker):
                roleStats = new RoleStats(0.85f, 0, 1.15f);
                if (isLocalPlayer) {
                    CmdProvideStats(roleStats.maxHealth, roleStats.resilience, roleStats.speed);
                    SetStats(roleStats.resilience, roleStats.speed);
                }
                //if (attributes.ContainsKey("DEF")) //increased resilience based on STR
                //    resilience = (int)Mathf.Ceil(0.2f * (int)attributes["DEF"]); // Do damage according to ATT aswell?
                //  Boomnana - (yup, same one) deals 80% of current health on enemy target in damage; of no targets are hit it return to the caster and deals 35% of current health damage. CD: 3sec
                abilities[0] = GetComponent<ThrowBoomnana>();
                //  Tail Slap - (yup, same one) deals 2% of current health on enemy target; melee; no CD; 1 sec to "cast"
                abilities[1] = GetComponent<TailSlap>();
                //  Punch Dance - deals a stronger tail slap (3% of current health damage) that if it hits stuns the enemy for 2 sec and it's followed by 2 more tail slaps of 4% and 5% damage*current health. CD:20 sec
                abilities[2] = GetComponent<PunchDance>();
                // Placeholder visual thing
                //body.GetComponent<MeshRenderer>().material.color = Color.red;
                foreach(Material m in body.GetComponent<MeshRenderer>().materials)
                    m.color = team == 1 ? Color.yellow : Color.blue;
                break;
            case (Role.Supporter):
                roleStats = new RoleStats(1f, 0, 1f);
                syncMaxHealth *= 1f;
                // Do something according to SUP? Do Resilience? Do ATT?
                //  Puke - (the old puke, does the same thing) stuns all enemies in range, has about 2 units distance units in range. Channeled 3 sec; CD:5 sec
                //  Throw poison - ranged ability, slows the enemy at 0,5*speed and deals 0.5% damage*max health over 3 sec (1.5% in total). Range from 5 to 30 distance units. No CD; takes 1 sec to cast and requires poisonous herbs
                //  Heal force - ability targets only friendly characters. Heals 50-250 HP over 3 sec depending on skill and herbs used in the ability. Max range 20 units. 1 herb heals instantly for 50HP, 2->4 herb heal over time (50 at first and 50 more for each 'tic'). No CD; instant application; requires herbs to cast
                speed *= 1f;
                // Placeholder visual thing
                body.GetComponent<MeshRenderer>().material.color = Color.green;
                break;
            default:
                body.GetComponent<MeshRenderer>().material.color = Color.white;
                abilities[0] = GetComponent<ShootBullet>();
                abilities[1] = GetComponent<HealSelf>();
                break;
        }
        gameObject.GetComponent<Rigidbody>().transform.localScale = new Vector3(sizeModifier, sizeModifier, sizeModifier);
        //gameObject.GetComponent<Rigidbody>().transform.localScale *= sizeModifier; // Applies on others
        maxHealth = syncMaxHealth;
        health = maxHealth;
    }

    [ClientCallback]
    void SelectRole() {
        if (isLocalPlayer) {
            CmdProvideRole(role);
        }
    }

    [ClientCallback]
    void TeamSelect() {
        body.GetChild(0).gameObject.SetActive(team == 1 ? true : false);
        body.GetChild(1).gameObject.SetActive(team == 2 ? true : false);
    }

    [Command]
    void CmdProvideStats(float maxHp, int resi, float spd) {
        ScoreManager SM = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
        syncMaxHealth = SM.compositeHealthFormula(team == 1 ? SM.teamOne : team == 2 ? SM.teamTwo : 0) * maxHp;
        maxHealth = syncMaxHealth;
        SetStats(resi, spd);
    }
    void SetStats(int resi, float spd) {
        resilience = 0 + resi;
        sizeModifier = (maxHealth / 1000);
        speed = 5f * spd;
    }

    [ClientCallback]
    void StatSync() {
        if (maxHealth != syncMaxHealth && isLocalPlayer) {
            CmdChangeHealth((syncMaxHealth / (1000 * roleStats.maxHealth)));
        }
        maxHealth = syncMaxHealth;
        health = syncHealth;
    }

    [Command]
    void CmdChangeHealth(float hp) {
        syncHealth = (1000 * roleStats.maxHealth) * hp;
    }

    [Command]
    void CmdRespawn() {
        Respawn();
    }

    void Respawn() {
        isDead = false;
        syncHealth = syncMaxHealth;
        transform.position = GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().GetSpawnPosition();
        GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
    }

    [Command]
    void CmdProvideRole(Role role) {
        syncRole = role;
    }

    /// <summary>
    /// ONLY USE THIS ON SERVER - CmdTakeDmg if you are anything else
    /// Logic function for taking damage
    /// Sets players to being dead (if they are at 0 or less health) and updates the ScoreManager
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDmg(float amount) { // amount == currSizeMaxHealth
        if (!isServer)
            return;
        GetComponent<Camouflage>().brokeStealth = true;
        syncHealth -= amount * (1.0f - ((float)resilience / 100.0f));
        if (syncHealth <= 0 && !isDead) {
            ScoreManager SM = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
            SM.CountDeaths(team);
            isDead = true;
            deathTimer = (float)Network.time;
            syncHealth = 0;
        }
        RpcTakeDmg(amount * (1.0f - ((float)resilience / 100.0f)));
    }
    /// <summary>
    /// ONLY USE THIS ON SERVER - CmdHealing if you are anything else
    /// Logic function for regenerating/recieving health
    /// Also caps health at maxHealth
    /// </summary>
    /// <param name="amount"></param>
    public void Healing(float amount) {
        if (!isServer)
            return;
        syncHealth += amount;
        if (syncHealth > maxHealth)
            syncHealth = maxHealth;
        RpcHealing(amount);
    }
    /// <summary>
    /// ONLY USE THIS ON SERVER
    /// Incapacitate the player - rendering them unable to move/control their character
    /// </summary>
    /// <param name="duration"></param>
    public void Stun(float duration) {
        if (!isServer)
            return;
        isStunned = true;
        stunTimer = Network.time + duration;
        RpcStunning(duration);
    }

    /// <summary>
    /// Issue a command to the server to deal damage to the player
    /// See the TakeDmg function for more details
    /// </summary>
    /// <param name="amount"></param>
    [Command]
    public void CmdTakeDmg(float amount) { TakeDmg(amount); }
    /// <summary>
    /// Issue a command to hte server to heal the player
    /// See the Healing function for more details
    /// </summary>
    /// <param name="amount"></param>
    [Command]
    public void CmdHealing(float amount) { Healing(amount); }

    /// <summary>
    /// Logic for when the player has connected and team stuff happens
    /// 
    /// Assigns the team to the current Player, Updates the ScoreManager and
    /// Updates the changeMaxHealth for all players on the same team
    /// </summary>
    /// <param name="joinedTeam">The team that the player joined</param>
    [Command]
    public void CmdTeamSelection(int joinedTeam) {
        team = joinedTeam;
        GameObject.Find("ScoreManager").GetComponent<ScoreManager>().TeamSelection(this);
        Debug.Log(gameObject.name + " joined team " + joinedTeam);
        foreach(PlayerStats ps in FindObjectsOfType<PlayerStats>())
            ps.changeMaxHealth = ps.team == joinedTeam ? true : ps.team == joinedTeam ? true : false;
        RpcTeam(team, gameObject.name);
    }

    /// <summary>
    /// LocalPlayer function re-calculates the RoleCharacteristics and 
    /// assigns the changeMaxHealth to false (because it was just changed)
    /// </summary>
    [ClientCallback]
    void NewPlayerJoinedTeam() {
        if (isLocalPlayer) {
            RoleCharacteristics(role);
            changeMaxHealth = false;
            CmdChangeMaxHealth(false);
        }
    }
    
    /// <summary>
    /// Hook Function for when syncMaxHealth changes
    /// Updates size and re-assigns syncMaxHealth
    /// </summary>
    /// <param name="hp"></param>
    public void SyncMaxHealth(float hp) {
        syncMaxHealth = hp;
        sizeModifier = (syncMaxHealth / 1000);
        gameObject.GetComponent<Rigidbody>().transform.localScale = new Vector3(sizeModifier, sizeModifier, sizeModifier); // Applies twice
    }

    /// <summary>
    /// Trigger a change of Terrain for the local player
    /// </summary>
    public void GenerateTerrain() {
        if (isServer) {
            makeMap = true;
            return;
        }
        if(isLocalPlayer) {
            GameObject.Find("mapHandler").GetComponent<mapCreator>().playerConnected();
            makeMap = false;
            CmdChangeMakeMap(false);
        }
    }

    /// <summary>
    /// Command for changing the SyncVar changeMaxHealth
    /// </summary>
    /// <param name="change"></param>
    [Command]
    void CmdChangeMaxHealth(bool change) {
        changeMaxHealth = change;
    }

    /// <summary>
    /// Update the SyncVar tied to adding rings in 'stinaScene_foolingaroundwithcircles
    /// </summary>
    /// <param name="change"></param>
    [Command]
    void CmdChangeMakeMap(bool change) {
        makeMap = change;
    }

    [ClientRpc]
    void RpcTakeDmg(float amount) {
        Debug.Log("Took damage: " + amount);
    }
    [ClientRpc]
    void RpcHealing(float amount) {
        Debug.Log("Recieved healing: " + amount);
    }
    [ClientRpc]
    void RpcStunning(float duration) {
        Debug.Log("Stunned for: " + duration);
    }
    [ClientRpc]
    void RpcTeam(int joinedTeam, string name) {
        Debug.Log(name + " joined team " + joinedTeam);
    }
}
