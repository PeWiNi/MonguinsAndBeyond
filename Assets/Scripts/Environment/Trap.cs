using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Trap : NetworkBehaviour
{
    [Tooltip("The duration of the Trap before it expires")]
    [Range(0, 60)]
    public int duration = 30;
    [Tooltip("The duration of the effect of the Trap")]
    public int effectDuration = 30;
    [Tooltip("The state of the Trap - Sprung or not")]
    public bool isTriggered = false;

    // Use this for initialization
    void Start()
    {

    }
}
