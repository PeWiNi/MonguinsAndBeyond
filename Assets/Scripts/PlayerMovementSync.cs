using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class PlayerMovementSync : NetworkBehaviour {

    [SyncVar]
    Vector3 syncPosition;
    [SyncVar]
    Quaternion syncPlayerRotation;
    [SyncVar]
    Quaternion syncCamRotation;

    [SerializeField]
    Transform playerTransform;
    [SerializeField]
    Transform camTransform;
    [SerializeField]
    float lerpRate = 10;
    
    PlayerLogic playerScript;

    void Start() {
        playerScript = GetComponent<PlayerLogic>();
    }

    void Update() {
        //LerpTransform();
        TransmitOrientation();
    }

    void LerpTransform() {
        if (!isLocalPlayer) {
            playerTransform.position = Vector3.Lerp(playerTransform.position, syncPosition, Time.deltaTime * lerpRate);
            playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, syncPlayerRotation, Time.deltaTime * lerpRate);
            camTransform.rotation = Quaternion.Lerp(camTransform.rotation, syncCamRotation, Time.deltaTime * lerpRate);
        }
    }

    void FixedUpdate() {
        if (isLocalPlayer) {
            playerScript.Simulate();
        }
        LerpTransform();
    }

    [Command]
    void CmdProvideOrientation(Vector3 pos, Quaternion playerRot, Quaternion camRot) {
        syncPlayerRotation = playerRot;
        syncCamRotation = camRot;
        syncPosition = pos;
    }

    [ClientCallback]
    void TransmitOrientation() {
        if (isLocalPlayer) {
            CmdProvideOrientation(playerTransform.position, playerTransform.rotation, camTransform.rotation);
        }
    }
}
