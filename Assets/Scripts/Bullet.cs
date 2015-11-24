using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {
    public float damage;
    public Color colour;

    // Use this for initialization
    void Start() {
        GetComponent<MeshRenderer>().material.color = colour;
    }

    void OnCollisionEnter(Collision _collision) {
        if (_collision.collider.tag == "Player") {
            _collision.transform.GetComponentInParent<PlayerStats>().CmdTakeDmg(damage);
            Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
        }
        Destroy(gameObject);
    }
}
