using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class PlayerMovementSync : NetworkBehaviour {

    [SyncVar]
    Vector2 syncInput;
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

    // represents a move command sent to the server
    private struct move {
        public float HorizontalAxis;
        public float VerticalAxis;
        public double Timestamp;
        public move(float horiz, float vert, double timestamp) {
            this.HorizontalAxis = horiz;
            this.VerticalAxis = vert;
            this.Timestamp = timestamp;
        }
    }

    // a history of move states sent from client to server
    List<move> moveHistory = new List<move>();
    // a reference to the Player script attached to the game object.
    PlayerLogic playerScript;
    OrbitCharacter orbitCharacter;

    void Start() {
        playerScript = GetComponent<PlayerLogic>();
        orbitCharacter = GetComponent<OrbitCharacter>();
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
            // get current move state
            move moveState = new move(playerScript.horizAxis, playerScript.vertAxis, Network.time);
            // buffer move state
            moveHistory.Insert(0, moveState);
            // cap history at 200
            if (moveHistory.Count > 200) {
                moveHistory.RemoveAt(moveHistory.Count - 1);
            }
            // simulate
            playerScript.Simulate();
            // send state to server
            //CmdProvideMovementToServer(moveState.HorizontalAxis, moveState.VerticalAxis, playerTransform.position);
        }
        LerpTransform();
    }

    /* Code to prevent speed-hacking, teleporting and high latency - didn't work 10/11-2015
    [Command]
    void CmdProvideMovementToServer(float hori, float vert, Vector3 pos) {
        syncInput = new Vector2(hori, vert);

        // compare results
        if (Vector3.Distance(playerTransform.position, pos) > 0.1f) {
            // error is too big, tell client to rewind and replay
            CmdCorrectState(pos, Network.time);
        }
    }

    [Command]
    void CmdCorrectState(Vector3 correctPosition, double timestamp) {
        // find past state based on timestamp
        int pastState = 0;
        for (int i = 0; i < moveHistory.Count; i++) {
            if (moveHistory[i].Timestamp <= timestamp) {
                pastState = i;
                break;
            }
        }

        // rewind
        playerTransform.position = correctPosition;
        // replay
        for (int i = pastState; i >= 0; i--) {
            playerScript.horizAxis = moveHistory[i].HorizontalAxis;
            playerScript.vertAxis = moveHistory[i].VerticalAxis;
            playerScript.Simulate();
        }

        //syncPosition = myTransform.position;

        // clear
        moveHistory.Clear();
    }*/

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
