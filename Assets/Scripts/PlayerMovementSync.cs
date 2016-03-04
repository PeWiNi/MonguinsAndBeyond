using System;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings (channel = 0, sendInterval = 0.1f)]
public class PlayerMovementSync : NetworkBehaviour {

    [SyncVar]// (hook = "SyncPositionValues")]
    Vector3 syncPosition;
    [SyncVar]
    float syncPlayerRotation;

    [SerializeField]
    Transform playerTransform;
    [SerializeField]
    float lerpRate = 10;

    Vector3 lastPos;
    float lastRot;
    DateTime lastTime = DateTime.Now;
    float posThreshold = 0.5f;
    float rotThreshold = 1f;
    TimeSpan timeThreshold = new TimeSpan(0, 0, 0, 0, 500);

    PlayerLogic playerScript;

    void Start() {
        playerScript = GetComponent<PlayerLogic>();
        //rgdbdy = playerTransform.GetComponent<Rigidbody>(); rgdbdy.velocity.y != 0;
    }

    void Update() {
        //ExtendedLerpTransform();
        TransmitOrientation();
    }

    void FixedUpdate() {
        if (isLocalPlayer) {
            playerScript.Simulate();
        }
        LerpTransform();
    }

    void LerpTransform() {
        if (!isLocalPlayer) {
            if (Vector3.Distance(syncPosition, playerTransform.position) > 5)
                playerTransform.position = syncPosition;
            playerTransform.position = Vector3.Lerp(playerTransform.position, syncPosition, Time.deltaTime * lerpRate);
            playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, Quaternion.Euler(new Vector3(0, syncPlayerRotation,0)), Time.deltaTime * lerpRate);
        }
    }

    [Command]
    void CmdProvideOrientation(Vector3 pos, float playerRot) {
        syncPlayerRotation = playerRot;
        syncPosition = pos;
        //print("Another Server Call...");
    }
    [ClientCallback]
    void TransmitOrientation() {
        if (isLocalPlayer && 
            (Vector3.Distance(playerTransform.position, lastPos) > posThreshold || //(lastPos.y - playerTransform.position.y) > 0) || 
            (Mathf.Abs(playerTransform.localEulerAngles.y - lastRot) > rotThreshold) || TimeExceeded())) {
            CmdProvideOrientation(playerTransform.position, playerTransform.localEulerAngles.y);
            lastPos = playerTransform.position;
            lastRot = playerTransform.localEulerAngles.y;
        }
    }

    bool TimeExceeded() {
        DateTime currentTime = DateTime.Now;
        bool result = currentTime.Subtract(lastTime) > timeThreshold;
        if (result)
            lastTime = currentTime;
        return result;
    }

    /* Anti-lag stuffs
    List<Vector3> syncPosList = new List<Vector3>();
    public bool isHistorical = false;
    float normalLerpRate = 16;
    float fasterLerpRate = 27;
    float syncThreshold = 0.11f;

    [Client]
    void SyncPositionValues(Vector3 pos) {
        syncPosition = pos;
        syncPosList.Add(syncPosition);
    }

    void ExtendedLerpTransform() {
        if (!isLocalPlayer) {
            if(!isHistorical)
                playerTransform.position = Vector3.Lerp(playerTransform.position, syncPosition, Time.deltaTime * lerpRate);
            else {
                if(syncPosList.Count > 0) {
                    playerTransform.position = Vector3.Lerp(playerTransform.position, syncPosList[0], Time.deltaTime * lerpRate);
                    if (Vector3.Distance(playerTransform.position, syncPosList[0]) < syncThreshold)
                        syncPosList.RemoveAt(0);
                    if (syncPosList.Count > 10)
                        lerpRate = fasterLerpRate;
                    else
                        lerpRate = normalLerpRate;
                }
            } //print("StackSize: " + syncPosList.Count);
            playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, syncPlayerRotation, Time.deltaTime * lerpRate);
        }
    }
    */
}
