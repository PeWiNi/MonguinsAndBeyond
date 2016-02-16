using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerStats : NetworkBehaviour {

    [SyncVar]
    float syncHealth;
    [SyncVar]
    int syncResilience;
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
    public float maxHealth = 1000f; // Full amount of health
    [SerializeField]
    public float health; // Reach 0 and you die
    [Range(0, 100)] [SerializeField]
    int resilience = 0; // Recieved damage modifier, 100 means 100% dmg reduction
    [SerializeField]
    private Hashtable attributes; // Strength, Agility, Wisdom, etc. and their respective values // SYNCVAR?
    [SerializeField]
    Role role = Role.Basic; // Current primary Role to determine abilities
    [Range(0.2f, 2.5f)] [SerializeField]
    public float sizeModifier = 1f;
    [Range(0.5f, 10f)] [SerializeField]
    public float speed = 5f; // Movement (and jumping) speed (see PlayerLogic.cs)
    [SerializeField]
    Transform body;
    [SyncVar]
    public int team;

    public Ability[] abilities;

    [SyncVar]
    public bool isStunned = false;
    [SyncVar]
    double stunTimer;

    public GameObject bulletPrefab;
    
    public enum Role {
        Basic, Defender, Attacker, Supporter
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
            attributes = GameObject.Find("NetworkManager").GetComponent<MyNetworkManagerHUD>().getAttributes();
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
            RoleCharacteristics(role); 
            #endregion
            CmdTeamSelection(GameObject.Find("NetworkManager").GetComponent<MyNetworkManagerHUD>().team);
        }
        syncHealth = maxHealth;
    }
	
	// Update is called once per frame
	void Update () {
        if (isDead) {
            Color c = body.GetComponent<MeshRenderer>().material.color;
            if (((float)Network.time - deathTimer) > deathCooldown) {
                body.GetComponent<MeshRenderer>().material.color = new Color(c.r, c.g, c.b, 1f);
                if (isLocalPlayer) {
                    CmdRespawn();
                }
                return;
            }
            health = 0;
            body.GetComponent<MeshRenderer>().material.color = new Color(c.r, c.g, c.b, .2f);
            return;
        } if (isStunned) {
            if (stunTimer < Network.time) {
                isStunned = false;
            }
        }
        TeamSelect();
        SelectRole();
        ApplyRole();
        StatSync();
    }

    void ApplyRole() {
        if (!isLocalPlayer) {
            if (role != syncRole) {
                role = syncRole;
                RoleCharacteristics(syncRole);
            }
        }
    }

    // Apply characteristics of Role - Taken from Wikia, last updated: 24th of November 2015
    void RoleCharacteristics(Role role) {
        abilities = new Ability[3];
        switch (role) {
            case (Role.Defender):
                maxHealth *= 1.3f;
                if (isLocalPlayer && attributes.ContainsKey("DEF")) //increased resilience based on STR
                    resilience = (int)Mathf.Ceil(0.2f * (int)attributes["DEF"]);
                // Add abilities: - and determine how they are set
                //  Taunt (Roar/Growl/WTV) - taunts enemies (locks their target on him for 3 sec) CD:6 sec
                //  Smash (deals 1% of enemy health and stuns 1 sec) - no CD, should take 1sec to fully cast anyway
                //  Fortify - temporarily increase health and resilience of the defender with 20% for 10 sec. CD:20sec
                sizeModifier *= 1.5f;
                speed *= 0.80f;
                // Placeholder visual thing
                body.GetComponent<MeshRenderer>().material.color = Color.blue;
                break;
            case (Role.Attacker):
                maxHealth *= 0.85f;
                //if (attributes.ContainsKey("DEF")) //increased resilience based on STR
                //    resilience = (int)Mathf.Ceil(0.2f * (int)attributes["DEF"]); // Do damage according to ATT aswell?
                // Add abilities: - and determine how they are set
                //  Boomnana - (yup, same one) deals 80% of current health on enemy target in damage; of no targets are hit it return to the caster and deals 35% of current health damage. CD: 3sec
                //  Tail Slap - (yup, same one) deals 2% of current health on enemy target; melee; no CD; 1 sec to "cast"
                //  Punch Dance - deals a stronger tail slap (3% of current health damage) that if it hits stuns the enemy for 2 sec and it's followed by 2 more tail slaps of 4% and 5% damage*current health. CD:20 sec
                sizeModifier *= 0.85f;
                speed *= 1.15f;
                // Placeholder visual thing
                body.GetComponent<MeshRenderer>().material.color = Color.red;
                abilities[0] = GetComponent<ThrowBoomnana>();
                abilities[1] = GetComponent<TailSlap>();
                abilities[2] = GetComponent<PunchDance>();
                break;
            case (Role.Supporter):
                maxHealth *= 1f;
                // Do something according to SUP? Do Resilience? Do ATT?
                // Add abilities: - and determine how they are set
                //  Puke - (the old puke, does the same thing) stuns all enemies in range, has about 2 units distance units in range. Channeled 3 sec; CD:5 sec
                //  Throw poison - ranged ability, slows the enemy at 0,5*speed and deals 0.5% damage*max health over 3 sec (1.5% in total). Range from 5 to 30 distance units. No CD; takes 1 sec to cast and requires poisonous herbs
                //  Heal force - ability targets only friendly characters. Heals 50-250 HP over 3 sec depending on skill and herbs used in the ability. Max range 20 units. 1 herb heals instantly for 50HP, 2->4 herb heal over time (50 at first and 50 more for each 'tic'). No CD; instant application; requires herbs to cast
                sizeModifier *= 1f;
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
        gameObject.GetComponent<Rigidbody>().transform.localScale *= sizeModifier;
        health = maxHealth;
        syncHealth = maxHealth;
    }

    [ClientCallback]
    void SelectRole() {
        if (isLocalPlayer) {
            CmdProvideRole(role);
        }
    }

    [ClientCallback]
    void StatSync() {
        health = syncHealth;
        if (isLocalPlayer) {
            CmdProvideStats(resilience);
        }
        if (!isLocalPlayer) {
            resilience = syncResilience;
        }
    }

    [ClientCallback]
    void TeamSelect() {
        body.GetChild(0).gameObject.SetActive(team == 1 ? true : false);
        body.GetChild(1).gameObject.SetActive(team == 2 ? true : false);
    }

    [Command]
    void CmdProvideStats(int resi) {
        //syncHealth = hp;
        syncResilience = resi;
    }

    [Command]
    void CmdRespawn() {
        isDead = false;
        syncHealth = maxHealth;
    }

    [Command]
    void CmdProvideRole(Role role) {
        syncRole = role;
    }

    [Command]
    public void CmdDoFire(float lifeTime) {
        GameObject bullet = (GameObject)Instantiate(
            bulletPrefab, transform.position + (transform.localScale.x * transform.forward),
            Quaternion.identity);

        bullet.GetComponent<Bullet>().setOwner(team);
        
        var bullet3D = bullet.GetComponent<Rigidbody>();
        bullet3D.velocity = transform.forward * 5f;
        Destroy(bullet, lifeTime);

        NetworkServer.Spawn(bullet);
    }

    
    public void TakeDmg(float amount) { // amount == currSizeMaxHealth
        if (!isServer)
            return;
        syncHealth -= amount * (1.0f - ((float)resilience / 100.0f));
        if (syncHealth <= 0 && !isDead) {
            isDead = true;
            deathTimer = (float)Network.time;
            syncHealth = 0;
        }
        RpcTakeDmg(amount * (1.0f - ((float)resilience / 100.0f)));
    }
    public void Healing(float amount) {
        if (!isServer)
            return;
        syncHealth += amount;
        if (syncHealth > maxHealth)
            syncHealth = maxHealth;
        RpcHealing(amount);
    }
    public void Stun(float duration) {
        isStunned = true;
        stunTimer = Network.time + duration;
        RpcStunning(duration);
    }

    [Command]
    public void CmdTakeDmg(float amount) {
        TakeDmg(amount);
    }
    [Command]
    public void CmdHealing(float amount) {
        Healing(amount);
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

    [Command]
    void CmdTeamSelection(int team) {
        this.team = team;
        RpcTeam(team);
    }
    [ClientRpc]
    void RpcTeam(float team) {
        Debug.Log("Joined Team: " + team);
    }
}
