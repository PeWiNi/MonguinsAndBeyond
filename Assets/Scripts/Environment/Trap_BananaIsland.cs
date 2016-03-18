using UnityEngine;
using System.Collections;

public class Trap_BananaIsland : Trap
{
    [Tooltip("The thrust force applied to the Player")]
    public float thrust = 20f;

    void OnTriggerEnter(Collider _collider)
    {
        if (_collider.transform.tag == "Player")
        {
            _collider.gameObject.GetComponent<Slip>().PlayerSlipped(this.effectDuration, this.thrust);
        }
    }
}
