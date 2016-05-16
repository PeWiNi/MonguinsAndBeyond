using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Dummy placeholder object for future pickup'ables
/// </summary>
public class Pickup : NetworkBehaviour {
    internal bool moveToUI = false;
    Transform GUIlocation;
    Camera cam;
    internal bool canCollide = true;
    float ElapsedTime;
    float FinishTime;

    internal PickupSpawner spawner;

    [SyncVar]
    public Transform owner = null;

    // Use this for initialization
    void Start () {
        if (moveToUI) {
            transform.Find("PickUpFX").gameObject.SetActive(false);
            return;
        }
        if (owner) {
            transform.position = doNotTouchTerrain(transform.position);
            if (Unreachable())
                ReturnToSender();
        }
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

    /// <summary>
    /// Crash Bandicoot inspired "animation" triggered when picking up items
    /// </summary>
    /// <param name="guiTransform">Target Location towards the transform</param>
    /// <param name="camera">The camera of the player</param>
    public void makeMoveGuy(Transform guiTransform, Camera camera) {
        moveToUI = true;
        GUIlocation = guiTransform;
        canCollide = false;
        cam = camera;
        ElapsedTime = 0;
        FinishTime = 2f;
    }

    /// <summary>
    /// Keeps the y from hitting the terrain (except in extreme conditions)
    /// </summary>
    /// <param name="pos">Current position of the object</param>
    /// <param name="distance">Y-distance from terrain</param>
    /// <returns>Position away from the terrain</returns>
    public Vector3 doNotTouchTerrain(Vector3 pos, float distance = 1.5f) {
        Vector3 hoverPos = pos;
        RaycastHit hit;
        if (Physics.Raycast(pos, -Vector3.up, out hit, Mathf.Infinity, ~(1 << 8))) {
            var distancetoground = hit.distance;
            var heightToAdd = transform.localScale.y * distance;
            hoverPos.y = pos.y - distancetoground + heightToAdd;
        } else if (Physics.Raycast(pos, Vector3.up, out hit, Mathf.Infinity, ~(1 << 8))) {
            var distancetoground = hit.distance;
            var heightToAdd = transform.localScale.y * distance;
            hoverPos.y = pos.y + distancetoground + heightToAdd;
        }
        return hoverPos;
    }

    void ReturnToSender() {
        transform.position = owner.transform.position;
    }

    bool Unreachable() {
        Collider[] hitCol = Physics.OverlapSphere(GetComponent<Collider>().transform.position, 0.1f);
        return hitCol.Length > 1;
    }

    public void SetSpawner(PickupSpawner pickupS) {
        spawner = pickupS;
    }

    void OnDestroy() {
        if(spawner != null) 
            spawner.TriggerSpawn();
    }
}
