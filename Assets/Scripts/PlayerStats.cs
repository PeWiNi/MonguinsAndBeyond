using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerStats : NetworkBehaviour {

    #region attributes & role
    [SerializeField]
    private Hashtable attributes; // Strength, Agility, Wisdom, etc. and their respective values

    [SyncVar]
    int resilience; // Defender stat, makes you tougher
    [SyncVar]
    int wisdom; // Supporter stat, increases the chance of good stuff happening
    [SyncVar]
    int agility; // Attacker stat, increases your combat efficiency
    public int Resilience { get { return resilience; } }
    public int Wisdom     { get { return wisdom; } }
    public int Agility    { get { return agility; } }

    [SyncVar]
    Role syncRole = Role.Basic;
    [SerializeField]
    Role role = Role.Basic; // Current primary Role to determine abilities

    public Ability[] abilities;

    RoleStats roleStats;
    #endregion

    #region stats affected by attributes
    [SyncVar(hook = "SyncMaxHealth")]
    float syncMaxHealth = 1500f;
    [SyncVar]
    float syncHealth;
    [SerializeField]
    public float maxHealth; // Full amount of health
    [SerializeField]
    public float health; // Reach 0 and you die
    [SyncVar]
    public bool changeMaxHealth = false;

    [Range(0.5f, 10f)]
    public float speed = 5f; // Movement (and jumping) speed (see PlayerLogic.cs)
    [SyncVar]
    public float damageModifier = 1f;
    [SyncVar]
    public float damageReduction = 1f;
    [SyncVar]
    public float syncSpeed;
    [SyncVar]
    public float sapModifier = 1f;

    [Range(0.2f, 2.5f)]
    [SyncVar]
    public float sizeModifier = 1f;
    #endregion

    #region Death
    [SyncVar]
    public bool isDead = false;
    [SyncVar]
    float deathTimer;
    public float deathCooldown = 15f;

    [SyncVar]
    public int kills;
    [SyncVar]
    public int assists;
    [SyncVar]
    public int deaths;
    #endregion

    [SerializeField]
    public Transform body;
    [SyncVar]
    public int team;

    #region Player states
    [SyncVar]
    public bool isIncapacitated = false;
    [SyncVar]
    double incapacitatedTimer;
    [SyncVar]
    public bool isStunned = false;
    [SyncVar]
    double stunTimer;
    [SyncVar]
    public bool isSlowed = false;
    [SyncVar]
    public bool isSapped = false;
    [SyncVar]
    double slowTime;
    [SyncVar]
    public double tauntedTime;
    [SyncVar]
    public Transform tauntedTarget;
    #endregion

    [SyncVar]
    bool makeMap = false;

    #region Materials
    public Material currentMaterial;
    [SerializeField]
    Material standardMat;
    [SerializeField]
    Material stealthMat;
    public Material standardMaterial { get { return standardMat; } }
    public Material stealthMaterial  { get { return stealthMat; } }
    #endregion

    //[SyncVar(hook = "SetServerInitTime")]
    //double serverInit;
    //[SyncVar]
    //float initSinking;
    
    public enum Role {
        Basic, Defender, Attacker, Supporter
    }

    public struct RoleStats {
        public float maxHealth;
        public float speed;
        public float dmgMultiplier;
        public float dmgReduction;
        public float sapModifier;
        public int resilience;
        public int wisdom;
        public int agility;

        public RoleStats(int resi, int agi, int wis) {
            print(resi + ", " + agi + ", " + wis);
            float mHp = 1f;
            // Resilience-health math (0..10 = +0-150, 11..35 = +150-
            mHp += (resi <= 10 ? (resi / 100) : resi <= 35 ? ((((float)(resi - 10) / 100) * .2f) + .1f) : resi > 35 ? ((((float)(resi - 36) / 100) * 0.234375f) + .15f) : 0);
            // Agility-health math
            mHp -= (agi > 35 ? ((float)(agi - 36) / 100) * 0.234375f : 0);

            float spd = 1f;
            // Resilience-speed math
            spd -= (resi > 35 ? ((float)(resi - 36) / 100) * 0.15625f : 0);
            // Agility-speed math
            spd += (agi > 35 ? ((float)(resi - 36) / 100) * 0.234375f : 0);

            float dmg = 1f;
            // Agility-damage math
            dmg += (agi <= 10 ? (agi / 100) : agi <= 35 ? ((((float)(agi - 10) / 100) * .4f) + .1f) : agi > 35 ? ((((float)(agi - 36) / 100) * 0.3125f) + .20f) : 0);

            float dmgR = 1f;
            dmgR -= (resi <= 10 ? (resi / 100) : resi <= 35 ? ((((float)(resi - 10) / 100) * .4f) + .1f) : resi > 35 ? ((((float)(resi - 36) / 100) * 0.3125f) + .20f) : 0);
            maxHealth = mHp;
            dmgMultiplier = dmg;
            dmgReduction = dmgR;
            sapModifier = 1 - (resi <= 10 ? (resi / 100) : resi <= 35 ? ((((float)(resi - 10) / 100) * .8f) + .10f) : resi > 35 ? ((((float)(resi - 36) / 100) * 0.3125f) + .30f) : 0);
            speed = spd;
            resilience = resi;
            agility = agi;
            wisdom = wis;
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
            case "RES":
                return Role.Defender;
            case "AGI":
                return Role.Attacker;
            case "WIS":
                return Role.Supporter;
            default:
                return Role.Basic;
        }
    }

	// Use this for initialization
	void Start () {
        syncSpeed = speed;
        if (isLocalPlayer) {
            #region Loading attributes and determining Role
            // Load Attributes set before entering game
            MenuScript NM = GameObject.Find("NetworkManager").GetComponent<MenuScript>();

            attributes = NM.getAttributes();
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
            #endregion
            //CmdTeamSelection(NM.team);
            CmdTeamSelection(NM.team > 0 ? NM.team : team);
            RoleCharacteristics(role);
            SelectRole();
            //try {
            HUDScript hud = GameObject.Find("HUD").GetComponent<HUDScript>();
            hud.SetPlayerStats(this);
            GetComponent<SyncInventory>().setInventory(hud.inventory);
            //} catch { }
        }
        currentMaterial = standardMaterial;
        syncHealth = syncMaxHealth;
    }
	
	// Update is called once per frame
	void Update () {
        if (isDead) { // Send to co-routine?
            if (((float)getServerTime() - deathTimer) > deathCooldown) {
                ChangeMaterial(false);
                Respawn();
                return;
            }
            health = 0;
            if (currentMaterial != stealthMat) {
                ChangeMaterial(true);
                Color c = body.GetComponent<SkinnedMeshRenderer>().material.color;
                body.GetComponent<SkinnedMeshRenderer>().material.color = new Color(c.r, c.g, c.b, .2f);
                GetComponent<Animator>().SetBool("IsAlive", false);
                GetComponent<NetworkAnimator>().SetTrigger("DeadByDamageTrigger");
            }
            return;
        } if (isStunned) 
            GetComponent<Rigidbody>().velocity = new Vector3();

        if (isServer)
            ServerStateCheck();
        
        TeamSelect();
        ApplyRole();
        StatSync();

        if(changeMaxHealth) { NewPlayerJoinedTeam(); }
        //if(makeMap) { GenerateTerrain(initSinking); }
        if (makeMap) { GenerateTerrain(); }
    }

    void ServerStateCheck() {
        if (isStunned) {
            if (stunTimer < getServerTime()) {
                isStunned = false;
            }
        }
        if (isIncapacitated) {
            if (incapacitatedTimer < getServerTime()) {
                isIncapacitated = false;
            }
        }
        if (slowTime < getServerTime()) {
            if (isSlowed) {
                Slow(false);
            }
        }
    }

    public float deathTimeLeft() {
        if (((float)getServerTime() - deathTimer) < deathCooldown)
            return (float)getServerTime() - deathTimer;
        return 1;
    }

    public float stunTimeLeft() {
        if (stunTimer > getServerTime())
            return (float)(stunTimer - getServerTime());
        return 1;
    }

    #region RoleStuff
    /// <summary>
    /// Set the characteristics of your selected role on other clients
    /// </summary>
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
        if (isLocalPlayer) {
            roleStats = new RoleStats((int)attributes["DEF"], (int)attributes["ATT"], (int)attributes["SUP"]);
            CmdProvideStats(roleStats.maxHealth, roleStats.dmgMultiplier, roleStats.dmgReduction, roleStats.sapModifier, roleStats.speed, roleStats.resilience, roleStats.agility, roleStats.wisdom);
            SetStats(roleStats.resilience, roleStats.agility, roleStats.wisdom, roleStats.speed, roleStats.dmgMultiplier, roleStats.dmgReduction, roleStats.sapModifier);
        }
        switch (role) {
            case (Role.Defender):
                //  Taunt (Roar/Growl/WTV) - taunts enemies (locks their target on him for 3 sec) CD:6 sec
                abilities[1] = GetComponent<Taunt>();
                //  Smash (deals 1% of enemy health and stuns 1 sec) - no CD, should take 1sec to fully cast anyway
                abilities[0] = GetComponent<Smash>();
                //  Fortify - temporarily increase health and resilience of the defender with 20% for 10 sec. CD:20sec
                abilities[2] = GetComponent<Fortify>();
                break;
            case (Role.Attacker):
                //  Boomnana - (yup, same one) deals 80% of current health on enemy target in damage; of no targets are hit it return to the caster and deals 35% of current health damage. CD: 3sec
                abilities[2] = GetComponent<ThrowBoomnana>();
                //  Tail Slap - (yup, same one) deals 2% of current health on enemy target; melee; no CD; 1 sec to "cast"
                abilities[0] = GetComponent<TailSlap>();
                //  Punch Dance - deals a stronger tail slap (3% of current health damage) that if it hits stuns the enemy for 2 sec and it's followed by 2 more tail slaps of 4% and 5% damage*current health. CD:20 sec
                abilities[1] = GetComponent<PunchDance>();
                break;
            case (Role.Supporter):
                //  Puke - (the old puke, does the same thing) stuns all enemies in range, has about 2 units distance units in range. Channeled 3 sec; CD:5 sec
                abilities[1] = GetComponent<Puke>();
                //  Throw poison - ranged ability, slows the enemy at 0,5*speed and deals 0.5% damage*max health over 3 sec (1.5% in total). Range from 5 to 30 distance units. No CD; takes 1 sec to cast and requires poisonous herbs
                abilities[0] = GetComponent<ThrowPoison>();
                //  Heal force - ability targets only friendly characters. Heals 50-250 HP over 3 sec depending on skill and herbs used in the ability. Max range 20 units. 1 herb heals instantly for 50HP, 2->4 herb heal over time (50 at first and 50 more for each 'tic'). No CD; instant application; requires herbs to cast
                abilities[2] = GetComponent<HealForce>();
                break;
            default:
                // Instead we should do the freeMode player stuff here
                goto case Role.Attacker;
        }
        gameObject.GetComponent<Rigidbody>().transform.localScale = new Vector3(sizeModifier, sizeModifier, sizeModifier);
        //gameObject.GetComponent<Rigidbody>().transform.localScale *= sizeModifier; // Applies on others
        maxHealth = syncMaxHealth;
        if(health > maxHealth)
            health = maxHealth;
    }

    /// <summary>
    /// Tell your role to the server
    /// </summary>
    [ClientCallback]
    void SelectRole() {
        if (isLocalPlayer) {
            CmdProvideRole(role);
        }
    }

    /// <summary>
    /// Enable/disable banana/fish on forehead based on team
    /// </summary>
    [ClientCallback]
    void TeamSelect() {
        try {
            GetComponent<VisualizeTeam>().ToggleForeheadItem(team);
        } catch { Debug.Log("Team visualization could not be achieved :("); }
    }

    /// <summary>
    /// Let the server determine your health along with setting other stats
    /// TODO: Implement the rest of the stats
    /// </summary>
    /// <param name="maxHp">The maximum health of your selected role</param>
    /// <param name="spd">The speed modifer from your role</param>
    /// <param name="dmg"></param>
    /// <param name="resi">The resilience from your attributes</param>
    /// <param name="agi"></param>
    /// <param name="wis"></param>
    [Command]
    void CmdProvideStats(float maxHp, float spd, float dmg, float dmgR, float sapEffect, int resi, int agi, int wis) {
        ScoreManager SM = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
        syncMaxHealth = SM.compositeHealthFormula(team == 1 ? SM.teamOne : team == 2 ? SM.teamTwo : 0) * maxHp;
        maxHealth = syncMaxHealth;
        SetStats(resi, agi, wis, spd, dmg, dmgR, sapEffect);
    }
    /// <summary>
    /// Set the current stats locally
    /// Currently updates resilience, size and speed
    /// </summary>
    /// <param name="resi">The modifier which determines dmg reduction</param>
    /// <param name="spd">Movement speed of the character</param>
    void SetStats(int resi, int agi, int wis, float spd, float dmg, float dmgR, float sapEffect) {
        resilience = resi;
        agility = agi;
        wisdom = wis;
        sizeModifier = (maxHealth / 1000);
        syncSpeed = speed * spd;
        damageModifier = dmg;
        damageReduction = dmgR;
        sapModifier = sapEffect;
        GetComponentInChildren<CharacterCamera>().parentHeight = sizeModifier;
    }

    /// <summary>
    /// Syncronize health (and maxHealth) and tell the server to update your health
    /// </summary>
    [ClientCallback]
    void StatSync() {
        if (maxHealth != syncMaxHealth && isLocalPlayer) {
            CmdChangeHealth((syncMaxHealth / (1000 * roleStats.maxHealth)), roleStats.maxHealth);
            health = syncHealth;
        }
        maxHealth = syncMaxHealth;
        health = syncHealth;
    }

    /// <summary>
    /// Tell the server your current health and make it update your syncHealth
    /// This is also where the health is determined whenever someone connects/disconnects
    /// TODO: Update based on previous maxHealth rather than the new one
    /// </summary>
    /// <param name="hp">Health modifier to be updated with</param>
    [Command]
    void CmdChangeHealth(float hp, float RSmaxHealth) {
        if(syncHealth < (1000 * RSmaxHealth) * hp) {
            float miniMe = syncHealth / syncMaxHealth;
            syncHealth = miniMe * (1000 * RSmaxHealth) * hp;
        } else syncHealth = (1000 * RSmaxHealth) * hp;
    }

    /// <summary>
    /// Update the SyncVar by telling your role to the server
    /// </summary>
    /// <param name="role">Your currently selected role</param>
    [Command]
    void CmdProvideRole(Role role) {
        syncRole = role;
    }
    #endregion

    /// <summary>
    /// Tell the server that you are about to respawn
    /// </summary>
    [Command]
    void CmdRespawn() {
        isDead = false;
        syncHealth = syncMaxHealth;
        Respawn();
    }

    /// <summary>
    /// Logic for respawning the player
    /// As the local player, tell the server that you are about to respawn
    /// Update the variables connected to coming back to life
    /// 
    /// Also for some reason you need to tell everybody your new position
    /// </summary>
    void Respawn() {
        if (isLocalPlayer)
        {
            CmdRespawn();
            ////Set to IsAlive parameter to True in the Animator.
            //GetComponent<Animator>().SetBool("IsDead", false);
            GetComponent<Animator>().SetBool("IsAlive", true);
            syncHealth = syncMaxHealth;
            transform.position = GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().GetSpawnPosition();
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        }
    }

    /// <summary>
    /// ONLY USE THIS ON SERVER - CmdTakeDmg if you are anything else
    /// Logic function for taking damage
    /// Sets players to being dead (if they are at 0 or less health) and updates the ScoreManager
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="attacker"></param>
    public void TakeDmg(float amount, Transform attacker) {
        if (!isServer)
            return;
        GetComponent<Camouflage>().brokeStealth = true;
        syncHealth -= amount * damageReduction;
        if (syncHealth <= 0 && !isDead) {
            ScoreManager SM = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
            SM.CountDeaths(team);
            isDead = true;
            deathTimer = (float)(getServerTime());
            syncHealth = 0;
            #region Individual Score
            if(attacker != null)
                attacker.GetComponent<PlayerStats>().kills++;
            //Do assist stuff
            deaths++;
            #endregion
        }
        RpcTakeDmg(amount * damageReduction);
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
    /// Stun the player - rendering them unable to move/control their character and stopping them from moving
    /// </summary>
    /// <param name="duration"></param>
    public void Stun(float duration) {
        if (!isServer)
            return;
        if (duration > 0) {
            isStunned = true;
            stunTimer = getServerTime() + duration;
            RpcStunning(duration);
        } else
            isStunned = false;
    }
    /// <summary>
    /// ONLY USE THIS ON SERVER
    /// Incapacitate the player - rendering them unable to move/control their character
    /// External forces can still move the player
    /// </summary>
    /// <param name="duration"></param>
    public void Incapacitate(float duration) {
        if (!isServer)
            return;
        if (duration > 0) {
            isIncapacitated = true;
            incapacitatedTimer = getServerTime() + duration;
            RpcIncapacitating(duration);
        } else
            isIncapacitated = false;
    }

    public void Taunt(GameObject user, float duration) {
        tauntedTarget = user.transform;
        tauntedTime = getServerTime() + duration;
    }

    public Transform isTaunted() {
        return tauntedTime > getServerTime() ? tauntedTarget : null;
    }

    public void Fortify(float value, float duration) {
        print(damageReduction);
        damageReduction -= value;
        StartCoroutine(TurnOffDamageReduction(value, duration));
        print(damageReduction);
    }

    IEnumerator TurnOffDamageReduction(float value, float time) {
        yield return new WaitForSeconds(time);
        print(damageReduction);
        damageReduction += value;
        print(damageReduction);
    }

    /// <summary>
    /// Issue a command to the server to deal damage to the player
    /// See the TakeDmg function for more details
    /// </summary>
    /// <param name="amount"></param>
    [Command]
    public void CmdTakeDmg(float amount) { TakeDmg(amount, null); }
    /// <summary>
    /// Issue a command to hte server to heal the player
    /// See the Healing function for more details
    /// </summary>
    /// <param name="amount"></param>
    [Command]
    public void CmdHealing(float amount) { Healing(amount); }
    
    public void Slow(bool slow, float time = 1, bool sap = false) {
        if (!isServer)
            return;
        if (slow)
            //slowTime = isSlowed ? getServerTime() : slowTime + time;
            slowTime = getServerTime() + time;
        isSlowed = slow;
        isSapped = sap;
    }
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
    //public void GenerateTerrain(float initTime) {
    public void GenerateTerrain() {
        if (isServer) {
            //initSinking = initTime;
            makeMap = true;
            return;
        }
        if(isLocalPlayer) {
            mapCreator MC = GameObject.Find("mapHandler").GetComponent<mapCreator>();
            MC.playerConnected();
            //MC.SetSinkTime(initTime);
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
    void RpcIncapacitating(float duration) {
        Debug.Log("Incapacitated for: " + duration);
    }
    [ClientRpc]
    void RpcTeam(int joinedTeam, string name) {
        Debug.Log(name + " joined team " + joinedTeam);
    }

    /// <summary>
    /// Degenrate health over time.
    /// </summary>
    /// <param name="degenerationAmount"></param>
    /// <param name="duration"></param>
    IEnumerator Degenerate(float degenerationAmount, float duration, Transform caster) {
        float countDown = duration;
        while (countDown > 0f) {
            //print("Auch! - tick of damage occured");
            yield return new WaitForSeconds(1.0f);
            TakeDmg((degenerationAmount / duration), caster);
            countDown--;
        }
    }
    public void BadBerry(float amount, float duration, Transform caster) {
        StartCoroutine(Degenerate(amount, duration, caster));
    }
    /// <summary>
    /// Regenrate health over time.
    /// </summary>
    /// <param name="regenerationAmount"></param>
    /// <param name="duration"></param>
    IEnumerator Regenerate(float regenerationAmount, float duration) {
        float countDown = duration;
        while (countDown > 0f) {
            //print("Soothing healing! :D");
            yield return new WaitForSeconds(1.0f);
            Healing((regenerationAmount / duration));
            countDown--;
        }
    }
    public void GoodBerry(float amount, float duration) {
        StartCoroutine(Regenerate(amount, duration));
    }

    [ClientRpc]
    public void RpcServerInitTime(double time) {
        if (isLocalPlayer)
            GameObject.Find("HUD").GetComponent<HUDScript>().SetupTimer(time);
    }

    [Command]
    void CmdEatBerry(string berryType) {
        EatBerry(berryType);
    }

    public void EatBerry(string berryType) {
        if (!isServer)
            CmdEatBerry(berryType);
        Herb berry = new Herb();
        berry.ChangeProperties(berryType, this);
        berry.EatIt(this);
    }

    public bool CanIMove() {
        return !isDead && !isStunned && !isIncapacitated;
    }

    /// <summary>
    /// Finally a networked time that actually works..
    /// </summary>
    /// <returns>The effin' difference in Network.time between you and the server</returns>
    double getServerTime() {
        return GetComponent<GameTime>().time;
    }

    public void ChangeMaterial(bool stealth) {
        if(stealth && currentMaterial != stealthMaterial) {
            currentMaterial = stealthMaterial;
            body.GetComponent<SkinnedMeshRenderer>().material = currentMaterial;
            GetComponent<VisualizeTeam>().ToggleForeheadItem(team, false);
        }
        else if (!stealth && currentMaterial != standardMaterial) {
            currentMaterial = standardMaterial;
            body.GetComponent<SkinnedMeshRenderer>().material = currentMaterial;
            GetComponent<VisualizeTeam>().ToggleForeheadItem(team, true);
        }
    }
}
