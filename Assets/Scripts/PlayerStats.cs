using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour {

    public float health = 1000f; // Reach 0 and you die
    [Range(0, 100)]
    public int resilience = 0; // Recieved damage modifier, 100 means 100% dmg reduction
    private Hashtable attributes; // Strength, Agility, Wisdom, etc. and their respective values
    public Role role = Role.Basic; // Current primary Role to determine abilities
    [Range(0.2f, 2.5f)]
    public float sizeModifier = 1f;
    [Range(0.5f, 10f)]
    public float speed = 5f; // Movement (and jumping) speed (see PlayerLogic.cs)
    private Transform body;

    public enum Role {
        Basic, Defender, Attacker, Supporter
    }

	// Use this for initialization
	void Start () {
        body = transform.Find("Capsule");
	}
	
	// Update is called once per frame
	void Update () {
	    switch(role) {
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
}
