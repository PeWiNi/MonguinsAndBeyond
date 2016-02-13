using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour {
//public class Bullet : MonoBehaviour {
    public float damage;
    public Color colour;
    [SyncVar]
    public int ownerTeam;

    // Use this for initialization
    void Start() {
        GetComponent<MeshRenderer>().material.color = colour;
    }

    void OnCollisionEnter(Collision _collision) {
        //if (_collision.collider.tag == "Player" && _collision.collider.GetComponentInParent<PlayerID>().transform.name != owner) {
        if (_collision.collider.tag == "Player" && _collision.collider.GetComponentInParent<PlayerStats>().team != ownerTeam) {
            _collision.transform.GetComponentInParent<PlayerStats>().CmdTakeDmg(damage);
        }
        if(_collision.collider.tag != "Ability")
            Destroy(gameObject);
        Physics.IgnoreCollision(_collision.collider, GetComponent<Collider>());
    }

    public void setOwner(int s) {
        ownerTeam = s;
    }
}
