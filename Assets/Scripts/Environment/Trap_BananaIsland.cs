using UnityEngine;
using System.Collections;

public class Trap_BananaIsland : Trap
{
    [Tooltip("The amount we want to rotate around oneself")]
    [Range(0f, 180f)]
    public float anglesPerSecond = 25f;
    [Tooltip("The thrust force applied to the Player")]
    public float thrust = 10f;

    // Use this for initialization
    void Start()
    {
        //TODO: Possibly optimize by fixing server rotation issues
        transform.rotation = Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
    }

    void OnTriggerEnter(Collider _collider)
    {
        if (_collider.transform.tag == "Player")
        {
            _collider.gameObject.GetComponent<Slip>().PlayerSlipped(this.effectDuration, this.thrust, this.anglesPerSecond);
        }
    }
}
