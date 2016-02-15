using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Boomnana : NetworkBehaviour {
    float damage;
    public GameObject owner;
    float speed;
    float fullDamage;
    float selfDamage;
    int ownerTeam;
    bool movingBack = false;
    Vector3 endpoint;

    // Use this for initialization
    void Start() {

    }

    public void setup(GameObject owner, float distance, float spd, float fullDmg, float selfDmg) {
        this.owner = owner;
        ownerTeam = owner.GetComponent<PlayerStats>().team;
        damage = owner.GetComponent<PlayerStats>().maxHealth;
        speed = spd;
        fullDamage = fullDmg;
        selfDamage = selfDmg;
        endpoint = owner.transform.position + (owner.transform.forward * distance);
        movingBack = false;
    }

    // Update is called once per frame
    void Update() {
        if (transform.position == endpoint) {
            movingBack = true;
        }
        if (movingBack) {
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            transform.position = Vector3.MoveTowards(transform.position, owner.gameObject.transform.position, speed);
        } else {
            transform.position = Vector3.MoveTowards(transform.position, endpoint, speed);
        }
    }

    void OnCollisionEnter(Collision _collision) {
        if (_collision.gameObject == owner && movingBack)
            _collision.transform.GetComponentInParent<PlayerStats>().TakeDmg(damage * selfDamage); // Nana is spawned and is on server, trigger on client
        else {
            if (_collision.collider.tag == "Player" && _collision.collider.GetComponentInParent<PlayerStats>().team != ownerTeam) {
                _collision.transform.GetComponentInParent<PlayerStats>().TakeDmg(damage * fullDamage); // Nana is spawned and is on server, trigger on client
            }
        }
        if (_collision.collider.tag != "Ability")
            Destroy(gameObject);
        Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
    }
}
