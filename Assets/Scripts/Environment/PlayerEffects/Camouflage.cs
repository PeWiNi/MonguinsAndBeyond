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
    // This should be affected based on the values of the Role selected (Supporter, Defender, Attacker).
    float visibilityRangeRadius = 10f;  //21-04-16: Maximum detectionRange is 7 (with 100 WIS)
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

    private IEnumerator spotted;

    // Use this for initialization
    void Start() {
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
        if (isStealthed && !movementLeeway() && !hasMovedFromCamouflagePoint) {
            duration = (stealthDuration + StealthFromAgility()) + (float)getServerTime(); //Set the duration. Scale with AGI
            CmdMoved(StealthFromAgility());
            StopCoroutine(CamouflageDuration());
            StartCoroutine(CamouflageDuration());
        }
    }

    float StealthFromAgility() {
        float agi = GetComponent<PlayerStats>().Agility;
        float stealthFromAgility = (agi <= 35 ? agi * 0.142857f : agi > 35 ? ((agi - 36) * 0.156250078125f) + 4.999995f : 0);
        return stealthFromAgility;
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
        isPartlySpotted = false;
    }
    
    [Command]
    void CmdMoved(float stealthFromAgility) {
        duration = (stealthDuration + stealthFromAgility) + (float)getServerTime(); //Set the duration.
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
            yield return new WaitForSeconds(0.05f);
            partlySpottedValue -= 0.01f;
        }
    }

    void changeColor(float alpha) {
        PlayerStats ps = GetComponent<PlayerStats>();
        ps.ChangeMaterial(alpha < 1);
        Color c = ps.body.GetComponent<SkinnedMeshRenderer>().material.color;
        ps.body.GetComponent<SkinnedMeshRenderer>().material.color = new Color(c.r, c.g, c.b, alpha);
    }
    #endregion

    /// <summary>
    /// Start the camouflage duration.
    /// When it ends the Player will appear again.
    /// </summary>
    /// <returns></returns>
    IEnumerator CamouflageDuration()
    {
        while (duration > getServerTime()) {
            yield return new WaitForSeconds(1f);
            if (!hasMovedFromCamouflagePoint)
                yield break;
        }
        Appear();
        CmdChangeIsCamouflaged(false);
        yield return null;
    }

    void UpdateVisibilityState() {
        if (isCamouflaged) {
            //float visibru = visibilityRangeRadius * GetComponent<PlayerStats>().sizeModifier; // Scaling according to player size
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, visibilityRangeRadius);
            int trespassers = hitColliders.Length;
            foreach (Collider _collider in hitColliders) {
                if (_collider.tag != "Player" || _collider.gameObject == gameObject || _collider.GetComponent<PlayerStats>() != GameObject.Find("HUD").GetComponent<HUDScript>().GetPlayerStats()) {
                    trespassers--;
                    continue;
                }
                int wis = _collider.GetComponent<PlayerStats>().Wisdom;
                float detectionRange = (wis > 10 && wis <= 35 ? ((wis - 10) * 0.12f) + 2f : wis > 35 ? ((wis - 36) * 0.03125f) + 5f : 2f);
                if (!isPartlySpotted &&  Vector3.Distance(gameObject.transform.position, _collider.transform.position) <= detectionRange) {
                    print("DISTANCE: " + Vector3.Distance(gameObject.transform.position, _collider.transform.position) + ", RANGE: " + detectionRange);
                    isPartlySpotted = true;
                    spotted = PartlySpotted();
                    StartCoroutine(spotted);
                } // Mayhaps friendly guys can see you?
            }
            if (trespassers == 0 && isPartlySpotted) {
                isPartlySpotted = false;
                StopCoroutine(spotted);
                Hide();
            }
        }
    }

    public float stealthTimeLeft(bool time = false) {
        if (duration > getServerTime()) {
            if (time)
                return (float)(duration - getServerTime());
            else return (float)(duration - getServerTime()) / (stealthDuration + StealthFromAgility());
        }
        return 1;
    }

    double getServerTime() {
        return GetComponent<GameTime>().time;
    }
}
