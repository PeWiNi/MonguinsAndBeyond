using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Dummy placeholder object for future pickup'ables
/// </summary>
public class Pickup : NetworkBehaviour {
    bool moveToUI = false;
    Transform GUIlocation;
    Camera cam;
    internal bool canCollide = true;
    float ElapsedTime;
    float FinishTime;

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(new Vector3(0, .5f, 0));
    }

    void FixedUpdate() {
        if (moveToUI) {
            //Get the location of the UI element
            Vector3 screenPoint = GUIlocation.position;

            //World space coordinates
            //Vector3 worldPos = cam.ScreenToWorldPoint(screenPoint) + cam.transform.forward * .1f;

            //Don't make it huge and fly in your face
            Ray pos = cam.ScreenPointToRay(screenPoint);
            Vector3 worldPos = pos.origin + pos.direction * (cam.GetComponent<CharacterCamera>().currentDistance * .7f);

            //Move towards the world space position
            ElapsedTime += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, worldPos, ElapsedTime / FinishTime);

            //Kill when it reaches the target
            if (transform.position == worldPos)
                Destroy(gameObject);
        }
    }

    public void makeMoveGuy(Transform guiTransform, Camera camera) {
        moveToUI = true;
        GUIlocation = guiTransform;
        canCollide = false;
        cam = camera;
        ElapsedTime = 0;
        FinishTime = 2f;
    }
}
