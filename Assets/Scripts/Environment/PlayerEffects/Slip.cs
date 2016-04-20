using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Slip : NetworkBehaviour
{
    public bool isSlipping = false;
    float thrust = 0f;
    Animator animLocal;

    /// <summary>
    /// Player slipped
    /// </summary>
    /// <param name="effectDuration"></param>
    /// <param name="thrust"></param>
    public void PlayerSlipped(int effectDuration, float thrust) {
        if (!isLocalPlayer || isServer)
            return;
        if (!isSlipping) {
            if (animLocal == null)
                animLocal = GetComponent<Animator>();
            animLocal.SetBool("AffectedByBananaPeelTrap", true);
            this.thrust = thrust;
            int resi = GetComponent<PlayerStats>().Resilience;
            float duration = effectDuration * (1f - (resi <= 10 ? (float)(resi / 100) / 2 : resi <= 35 ? ((((float)(resi - 10) / 100) * .2f) + .05f) : resi > 35 ? ((((float)(resi - 36) / 100) * 0.15625f) + .10f) : 0));
            print(effectDuration * (1f - (resi <= 10 ? (float)(resi / 100) / 2 : resi <= 35 ? ((((float)(resi - 10) / 100) * .2f) + .05f) : resi > 35 ? ((((float)(resi - 36) / 100) * 0.15625f) + .10f) : 0)));
            StartCoroutine(Slipping(duration, thrust));
        }
    }

    IEnumerator Slipping(float effectDuration, float thrust) {
        isSlipping = true;
        transform.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * thrust, ForceMode.Impulse);
        CmdStun(effectDuration);
        while (effectDuration > 0) {
            yield return new WaitForSeconds(1f);
            effectDuration--;
        }
        animLocal.SetBool("AffectedByBananaPeelTrap", false);
        isSlipping = false;
    }

    [Command]
    void CmdStun(float effectDuration) {
        transform.GetComponent<PlayerStats>().Incapacitate(effectDuration);
    }
}
