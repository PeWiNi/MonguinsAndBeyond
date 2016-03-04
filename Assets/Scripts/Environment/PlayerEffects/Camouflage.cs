using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// --As pitched by Henrik to Claus 25/02--
/// Upon picking up leaves the user turns invisible*
/// The camouflage effect stats wearing off when the user moves
/// The player should be able to be detected if an enemy is near
/// When (damaged || cast ability || pickup/interact) stealth is broken
/// THOUGHT: Should detectionRange be based off detector or user
/// THOUGHT: Should teammates be able to see the hidden fella
/// 
/// *Also add magical particle effect of leafy doom
/// </summary>
public class Camouflage : NetworkBehaviour
{
    [SerializeField]
    public float visibilityRangeRadius = 20f;// This should be affected based on the values of the Role selected (Supportor, Defender, Attacker).
    [SerializeField]
    public float stealthDuration = 15f;
    [SyncVar]
    Vector3 camouflagePosition;
    [SyncVar]
    public bool isCamouflaged = false;
    [SyncVar]
    float duration = 0f;
    [SyncVar]
    double pickedUpTime;
    [SyncVar]
    public bool brokeStealth = false;
    float pickUpDelay = 1f;
    bool isStealthed = false;
    [SyncVar]
    public bool hasMovedFromCamouflagePoint = false;
    public bool isPartlySpotted = false;
    
    // Use this for initialization
    void Start()
    {
        InvokeRepeating("UpdateVisibilityState", 1f, 1f);
    }

    // Update is called once per frame
    void Update() {
        if (isServer)
            return;
        //Check whether the Player wants to become camouflaged or not.
        if (isCamouflaged && !isStealthed) { // Hide / Vanish / Disappear
            Hide();
        }
        if((!isCamouflaged && isStealthed) || brokeStealth) { // Appear / isVisibru
            Appear();
            CmdChangeIsCamouflaged(false);
        }
        //Should Check whether the Player has moved or used and Ability.
        if (!movementLeeway() && !hasMovedFromCamouflagePoint && pickedUpTime + pickUpDelay < Network.time) {
            duration = stealthDuration + (float)Network.time; //Set the duration. THOUGHT: scale according to size
            CmdMoved();
            StopCoroutine(CamouflageDuration());
            StartCoroutine(CamouflageDuration());
        }
    }

    bool movementLeeway() {
        Collider[] hitColliders = Physics.OverlapSphere(camouflagePosition, 1);
        foreach (Collider _collider in hitColliders)
            if (_collider.gameObject == gameObject)
                return true;
        return camouflagePosition == gameObject.transform.position;
    }

    [Command]
    void CmdChangeIsCamouflaged(bool value) {
        brokeStealth = value;
        isCamouflaged = value;
    }

    /// <summary>
    /// Camouflage the Player.
    /// </summary>
    public void BeginCamouflage()
    {
        if (!isServer)
            return;
        brokeStealth = false;
        isCamouflaged = true; //The Player is now camouflaged.
        camouflagePosition = gameObject.transform.position; //Set the current player position to be the camouflage position.
        hasMovedFromCamouflagePoint = false;
    }
    
    [Command]
    void CmdMoved() {
        duration = stealthDuration + (float)Network.time; //Set the duration. THOUGHT: scale according to size
        hasMovedFromCamouflagePoint = true;
    }

    #region Change transparency
    /// <summary>
    /// Make the Player disappear.
    /// </summary>
    void Hide() {
        float transity = .0f;
        if (isLocalPlayer) transity = .4f;
        changeColor(transity);
        if (!isLocalPlayer) GetComponentInChildren<Canvas>().enabled = false;
        isStealthed = true;
    }

    /// <summary>
    /// Make the Player appear again.
    /// </summary>
    void Appear() {
        changeColor(1f);
        isStealthed = false;
        if (!isLocalPlayer) GetComponentInChildren<Canvas>().enabled = true;
    }

    /// <summary>
    /// The Player will be 'almost' spotted resulting in being more vivisble.
    /// </summary>
    /// <returns></returns>
    IEnumerator PartlySpotted()
    {
        float partlySpottedValue = 0.8f;
        float visibilityValue = 0.2f;
        while (partlySpottedValue > visibilityValue) {
            changeColor(partlySpottedValue);
            yield return new WaitForSeconds(0.1f);
            partlySpottedValue -= 0.1f;
        }
    }

    void changeColor(float alpha) {
        Color c = GetComponent<PlayerStats>().body.GetComponent<MeshRenderer>().material.color;
        foreach (Material m in GetComponent<PlayerStats>().body.GetComponent<MeshRenderer>().materials) m.color = new Color(c.r, c.g, c.b, alpha);
    }
    #endregion

    /// <summary>
    /// Start the camouflage duration.
    /// When it ends the Player will appear again.
    /// </summary>
    /// <returns></returns>
    IEnumerator CamouflageDuration()
    {
        while (duration > Network.time) {
            yield return new WaitForSeconds(1f);
            if (!hasMovedFromCamouflagePoint)
                yield break;
        }
        Appear();
        CmdChangeIsCamouflaged(false);
        yield return null;
    }

    void UpdateVisibilityState() //mayhaps optimize with while loops
    {
        if (isCamouflaged && !isLocalPlayer) // If you want it WoW-stealth-like the player themselves shouldn't have the visibru effect when enemies are near
        {
            float visibru = visibilityRangeRadius * GetComponent<PlayerStats>().sizeModifier; // Scaling according to player size
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, visibru);
            foreach ( Collider _collider in hitColliders) {
                if (_collider.tag != "Player")
                    continue;
                if (!isPartlySpotted && _collider.transform.GetComponent<PlayerStats>().team != GetComponent<PlayerStats>().team && 
                    Vector3.Distance(gameObject.transform.position, _collider.transform.position) <= visibru) {
                    print("Enemy is wthin spot range! GET AWAY OR YOU WILL BE SPOTTED!!!");
                    isPartlySpotted = true;
                    StartCoroutine(PartlySpotted());
                }
                else if (_collider.transform.GetComponent<PlayerStats>().team != GetComponent<PlayerStats>().team 
                    && Vector3.Distance(gameObject.transform.position, _collider.transform.position) > visibru) {
                    isPartlySpotted = false;
                    StopCoroutine(PartlySpotted());
                    Hide();
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (isCamouflaged) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(gameObject.transform.position, visibilityRangeRadius * GetComponent<PlayerStats>().sizeModifier);
        }
    }
}
