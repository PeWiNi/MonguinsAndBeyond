using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Fish : Pickup
{
    public float amount = 0.15f;

    void OnTriggerEnter(Collider _collider) {
        if (!canCollide)
            return;
        if (_collider.tag == "Player") {
            PlayerStats ps = _collider.gameObject.GetComponent<PlayerStats>();
            ps.Healing(amount * ps.maxHealth);
            
            Destroy(gameObject);
        }
    }
}
