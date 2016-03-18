using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Slip : NetworkBehaviour
{
    public bool isSlipping = false;
    float thrust = 0f;

    /// <summary>
    /// Player slipped
    /// </summary>
    /// <param name="effectDuration"></param>
    /// <param name="thrust"></param>
    public void PlayerSlipped(int effectDuration, float thrust)
    {
        if (!isLocalPlayer || isServer)
            return;
        if (!isSlipping)
        {
            this.thrust = thrust;
            StartCoroutine(Slipping(effectDuration, thrust));
        }
    }

    IEnumerator Slipping(int effectDuration, float thrust) {
        isSlipping = true;
        transform.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * thrust, ForceMode.Impulse);
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
