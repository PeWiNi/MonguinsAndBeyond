using UnityEngine;
using System.Collections;

public class Trap_BananaIsland : Trap
{
    [Tooltip("The thrust force applied to the Player")]
    public float thrust = 20f;
    float modifier = 1f;

    void OnTriggerEnter(Collider _collider)
    {
        if (_collider.transform.tag == "Player")
        {
            _collider.gameObject.GetComponent<Slip>().PlayerSlipped(effectDuration, thrust * modifier);
        }
    }

    public void SetForceModifier(int wisdom) {
        modifier += wisdom <= 10 ?  wisdom / 100 : wisdom <= 35 ? (((float)(wisdom - 10) / 100) * .2f) + .1f : (((float)(wisdom - 36) / 100) * .15625f) + .15f;
    }
}
