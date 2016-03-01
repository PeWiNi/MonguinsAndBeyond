using UnityEngine;
using System.Collections;

public class Herb : Pickup
{
    void OnTriggerEnter(Collider _collider)
    {
        _collider.gameObject.GetComponent<HealthCondition>().RandomCondition();
        Destroy(gameObject);
    }
}
