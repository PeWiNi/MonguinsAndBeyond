using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Slip : NetworkBehaviour
{
    public bool isSlipping = false;
    float anglesPerSecond = 0f;
    float thrust = 0f;

    // Use this for initialization
    void Start()
    {

    }

    void Update()
    {
        if (isSlipping)
        {
            transform.Rotate(Vector3.up, anglesPerSecond);
        }
    }

    void FixedUpdate()
    {
        if (isSlipping)
        {
            transform.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * thrust);
        }
    }

    /// <summary>
    /// Player slipped
    /// </summary>
    /// <param name="effectDuration"></param>
    /// <param name="thrust"></param>
    /// <param name="anglesPerSecond"></param>
    public void PlayerSlipped(int effectDuration, float thrust, float anglesPerSecond)
    {
        if (!isLocalPlayer || isServer)
            return;
        if (!isSlipping)
        {
            this.thrust = thrust;
            this.anglesPerSecond = anglesPerSecond;
            StartCoroutine(Slipping(effectDuration, thrust, anglesPerSecond));
        }
    }

    IEnumerator Slipping(int effectDuration, float thrust, float anglesPerSecond) {
        isSlipping = true;
        CmdStun(effectDuration);
        while (effectDuration > 0) {
            yield return new WaitForSeconds(1f);
            effectDuration--;
        }
        isSlipping = false;
    }

    [Command]
    void CmdStun(int effectDuration) {
        transform.GetComponent<PlayerStats>().Stun(effectDuration);
    }
}
