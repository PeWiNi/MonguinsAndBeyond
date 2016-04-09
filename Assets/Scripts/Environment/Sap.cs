using UnityEngine;
using System.Collections;

public class Sap : Pickup {
    bool moveToUI = false;
    Transform GUIlocation;
    Camera cam;
    bool canCollide = true;

    public int sap = 1;

    void OnTriggerEnter(Collider _collider) {
        if (!canCollide)
            return;
        _collider.GetComponent<SyncInventory>().pickupSap(sap);
        Destroy(gameObject);
    }

    void FixedUpdate() {
        if (moveToUI) {
            //Get the location of the UI element
            Vector3 screenPoint = GUIlocation.position;
            //World space coordinates
            //Vector3 worldPos = cam.ScreenToWorldPoint(screenPoint) + cam.transform.forward * .1f;

            //Don't make it huge and fly in your face
            Ray pos = cam.ScreenPointToRay(screenPoint);
            Vector3 worldPos = pos.origin + pos.direction * cam.GetComponent<CharacterCamera>().currentDistance;

            //Move towards the world space position
            transform.position = Vector3.MoveTowards(transform.position, worldPos, .1f);

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
    }
}
