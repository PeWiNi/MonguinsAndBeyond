using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Boomnana : NetworkBehaviour {
    float damage;
    public GameObject owner;
    public float distance = 12f;
    public float speed = 1f;
    [Range(0, 1)]
    public float fullDamage = 0.8f;
    [Range(0, 1)]
    public float selfDamage = 0.35f;
    int ownerTeam;
    bool movingBack = false;
    public Vector3 endpoint;
    // Use this for initialization
    void Start() {

    }

    public void setup(GameObject owner) {
        this.owner = owner;
        ownerTeam = owner.GetComponent<PlayerStats>().team;
        damage = owner.GetComponent<PlayerStats>().maxHealth;
        endpoint = owner.transform.position + (owner.transform.forward * distance); // set actual endpoint according to spawn position
        movingBack = false;
    }

    // Update is called once per frame
    void Update() {
        //var bullet3D = GetComponent<Rigidbody>();
        if (transform.position == endpoint) {
            movingBack = true;
        }
        if (movingBack) {
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            transform.position = Vector3.MoveTowards(transform.position, owner.gameObject.transform.position, speed);
            //bullet3D.velocity = Vector3.MoveTowards(transform.position, owner.gameObject.transform.position, 1.0f) * 5f;
        } else {
            transform.position = Vector3.MoveTowards(transform.position, endpoint, speed);
            //bullet3D.velocity = Vector3.MoveTowards(transform.position, endpoint, 1.0f) * 5f;
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
