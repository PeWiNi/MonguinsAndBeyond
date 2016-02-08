using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {
    public float damage;
    public Color colour;
    public string owner;

    // Use this for initialization
    void Start() {
        GetComponent<MeshRenderer>().material.color = colour;
    }

    void OnCollisionEnter(Collision _collision) {
        Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
        if (_collision.collider.tag == "Player" && _collision.collider.GetComponentInParent<PlayerID>().transform.name != owner) {
            _collision.transform.GetComponentInParent<PlayerStats>().CmdTakeDmg(damage);
        }
        if(_collision.collider.tag != "Ability")
            Destroy(gameObject);
    }

    public void setOwner(string s) {
        owner = s;
    }
}
