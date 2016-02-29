using UnityEngine;
using System.Collections;

/// <summary>
/// Camouflage Leaf-pickup that will camouflage the player who 'picks it up'
/// </summary>
public class Leaf : Pickup {
    
    void OnTriggerEnter(Collider _collider) {
        _collider.gameObject.GetComponent<Camouflage>().BeginCamouflage();
        Destroy(gameObject);
    }
}
