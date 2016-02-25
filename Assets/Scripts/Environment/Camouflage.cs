﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// --As pitched by Henrik to Claus 25/02--
/// Upon picking up leaves the user turns invisible*
/// The camouflage effect wears off when the user moves
/// The player should be able to be detected if an enemy is near
/// When damaged stealth is broken
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
    public float stealthDuration = 5f;
    [SyncVar]
    Vector3 camouflagePosition;
    [SyncVar]
    public bool isCamouflaged = false;
    [SyncVar]
    float duration = 0f;
    [SyncVar]
    double pickedUpTime;
    float pickUpDelay = 1f;
    bool isStealthed = false;
    public bool hasMovedFromCamouflagePoint = false;
    public bool isPartlySpotted = false;
    
    // Use this for initialization
    void Start()
    {
        InvokeRepeating("UpdateVisibilityState", 1f, 1f);
    }

    // Update is called once per frame
    void Update() {
        //Check whether the Player wants to become camouflaged or not.
        if (isCamouflaged && !isStealthed) { // Hide / Vanish / Disappear
            Hide();
        }
        if(!isCamouflaged && isStealthed) { // Appear / isVisibru
            Appear();
        }
        //Should Check whether the Player has moved or used and Ability.
        if (pickedUpTime + pickUpDelay < Network.time && 
            (!movementLeeway() && !hasMovedFromCamouflagePoint)) {
            CmdMoved();
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

    /// <summary>
    /// Camouflage the Player.
    /// </summary>
    [Command]
    public void CmdBeginCamouflage()
    {
        isCamouflaged = true; //The Player is now camouflaged.
        duration = stealthDuration + (float)Network.time; //Set the duration. THOUGHT: scale according to size
        camouflagePosition = gameObject.transform.position; //Set the current player position to be the camouflage position.
        pickedUpTime = Network.time;
        hasMovedFromCamouflagePoint = false;
    }
    
    [Command]

    void CmdMoved() {
        hasMovedFromCamouflagePoint = true;
    }

    #region Change transparency
    /// <summary>
    /// Make the Player disappear.
    /// </summary>
    void Hide() {
        float transity = .0f;
        if (isLocalPlayer) transity = .4f;
        Color c = GetComponent<PlayerStats>().body.GetComponent<MeshRenderer>().material.color;
        GetComponent<PlayerStats>().body.GetComponent<MeshRenderer>().material.color = new Color(c.r, c.g, c.b, transity);
        if (!isLocalPlayer) GetComponentInChildren<Canvas>().enabled = false;
        isStealthed = true;
    }

    /// <summary>
    /// Make the Player appear again.
    /// </summary>
    void Appear() {
        Color c = GetComponent<PlayerStats>().body.GetComponent<MeshRenderer>().material.color;
        GetComponent<PlayerStats>().body.GetComponent<MeshRenderer>().material.color = new Color(c.r, c.g, c.b, 1f);
        isStealthed = false;
        GetComponentInChildren<Canvas>().enabled = true;
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
            Color c = GetComponent<PlayerStats>().body.GetComponent<MeshRenderer>().material.color;
            GetComponent<PlayerStats>().body.GetComponent<MeshRenderer>().material.color = new Color(c.r, c.g, c.b, partlySpottedValue);
            yield return new WaitForSeconds(0.1f);
            partlySpottedValue -= 0.1f;
        }
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
        }
        Appear();
        isCamouflaged = false;
        yield return null;
    }

    void UpdateVisibilityState() //mayhaps optimize with while loops
    {
        if (isCamouflaged)
        {
            float visibru = visibilityRangeRadius * GetComponent<PlayerStats>().sizeModifier; // Scaling according to player size
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, visibru);
            foreach ( Collider _collider in hitColliders) {
                if (_collider.tag != "Player")
                    continue;
                if (!isPartlySpotted && _collider.transform.GetComponent<PlayerStats>().team != GetComponent<PlayerStats>().team && 
                    Vector3.Distance(gameObject.transform.position, _collider.transform.position) <= visibru / 2) {
                    print("Enemy is wthin spot range! GET AWAY OR YOU WILL BE SPOTTED!!!");
                    isPartlySpotted = true;
                    StartCoroutine(PartlySpotted());
                }
                else if (_collider.transform.GetComponent<PlayerStats>().team != GetComponent<PlayerStats>().team 
                    && Vector3.Distance(gameObject.transform.position, _collider.transform.position) > visibru / 2) {
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
