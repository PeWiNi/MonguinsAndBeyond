using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Trap : NetworkBehaviour
{
    [Tooltip("The duration of the Trap before it expires")]
    [Range(0, 60)]
    [SyncVar]
    public int duration = 30;
    [Tooltip("The duration of the effect of the Trap")]
    [SyncVar]
    public int effectDuration = 30;
    [Tooltip("Checks whether the Trap has been triggered")]
    [SyncVar]
    public bool isTriggered = false;
    [Tooltip("Determines whether or not this Trap is under construction by a Player")]
    [SyncVar]
    public bool isUnderConstruction = false;
    [Tooltip("Determines whether the Trap is fully assembled")]
    [SyncVar]
    public bool isAssembled = false;
    [SyncVar]
    public int triggerCount;
    [SyncVar]
    public bool trigger = false;

    [SyncVar]
    public Transform owner;

    void Update () {
        if (trigger && triggerCount == 0)
            Destroy(gameObject);
    }

    public void SetOwner(Transform player) {
        owner = player;
    }

    public void SetTriggerCount(int count) {
        triggerCount = count;
        trigger = true;
    }

    [Command]
    public void CmdDecrementTrigger(int count) {
        triggerCount -= count;
    }
    public void DecrementTrigger() {
        triggerCount--;
    }
}
