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

    // Use this for initialization
    void Start()
    {

    }
}
