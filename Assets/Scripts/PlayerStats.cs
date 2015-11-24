using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerStats : NetworkBehaviour {

    [SyncVar]
    float syncHealth;
    [SyncVar]
    Role syncRole = Role.Basic;

    public float health = 1000f; // Reach 0 and you die
    public float maxHealth = 1000f; // 
    [Range(0, 100)]
    public int resilience = 0; // Recieved damage modifier, 100 means 100% dmg reduction
    private Hashtable attributes; // Strength, Agility, Wisdom, etc. and their respective values
    [SerializeField]
    Role role = Role.Basic; // Current primary Role to determine abilities
    [Range(0.2f, 2.5f)]
    public float sizeModifier = 1f;
    [Range(0.5f, 10f)]
    public float speed = 5f; // Movement (and jumping) speed (see PlayerLogic.cs)
    [SerializeField]
    Transform body;
    public GameObject bulletPrefab;

    public enum Role {
        Basic, Defender, Attacker, Supporter
    }

	// Use this for initialization
	void Start () {
        syncHealth = health;
        //body = transform.Find("Capsule");
        RoleCharacteristics(role);
	}
	
	// Update is called once per frame
	void Update () {
        //SelectRole();
        ApplyRole();
        if (isLocalPlayer)
            if (Input.GetKeyDown(KeyCode.E))
                CmdDoFire(3.0f);
    }

    void ApplyRole() {
        health = syncHealth;
        if (!isLocalPlayer) {
            if (role != syncRole) {
                role = syncRole;
                RoleCharacteristics(syncRole);
            }
        }
        if(isLocalPlayer) { }
    }

    [Command]
    void CmdProvideStats(float hp, Role role) {
        syncHealth = hp;
        syncRole = role;
    }

    [ClientCallback]
    void SelectRole() {
        if (isLocalPlayer) {
            CmdProvideStats(health, role);
        }
    }

    void RoleCharacteristics(Role role) {
        // Apply characteristics of Role
        switch (role) {
            case (Role.Defender):
                body.GetComponent<MeshRenderer>().material.color = Color.blue;
                break;
            case (Role.Attacker):
                body.GetComponent<MeshRenderer>().material.color = Color.red;
                break;
            case (Role.Supporter):
                body.GetComponent<MeshRenderer>().material.color = Color.green;
                break;
            default:
                body.GetComponent<MeshRenderer>().material.color = Color.white;
                break;
        }
    }

    [Command]
    void CmdDoFire(float lifeTime) {
        GameObject bullet = (GameObject)Instantiate(
            bulletPrefab, transform.position + (transform.forward),
            Quaternion.identity);

        var bullet3D = bullet.GetComponent<Rigidbody>();
        bullet3D.velocity = transform.forward * 5f;
        Destroy(bullet, lifeTime);

        NetworkServer.Spawn(bullet);
    }

    [Command]
    public void CmdTakeDmg(float damage) {
        syncHealth -= damage;
    }
}
