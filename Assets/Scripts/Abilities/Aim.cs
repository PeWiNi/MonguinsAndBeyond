using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Aim : NetworkBehaviour { // Future TODO: Fuse with SpawnTraps.cs
    GameObject projector;

    float distance;
    bool active = false;
    public bool aiming {
        get {
            return active;
        }
    }
    [Range(0, 1)]
    public float castAngles = 0f;

    // Use this for initialization
    void Start() {
        projector = (GameObject)Instantiate(Resources.Load("Prefabs/Aim_Projector") as GameObject, transform.position,
            Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));
        Activate(false);
    }

    // Update is called once per frame
    void Update() {
        if (aiming) {
            Vector3 pos = MidDist(PlaceStuff());
            projector.transform.position = pos;
            // Rotate according to mouse position
            projector.transform.eulerAngles = new Vector3(90, Quaternion.LookRotation(pos - 
                doNotTouchTerrain(transform.position)).eulerAngles.y - 90, projector.transform.eulerAngles.z);
        }
    }

    Vector3 MidDist(Vector3 pos) {
        Vector3 position = pos;
        Vector3 me = doNotTouchTerrain(transform.position);
        Ray ray = new Ray(me, (pos - me).normalized);
        position = ray.GetPoint(distance / 2);
        //Debug.DrawRay(ray.origin, ray.direction * distance / 2, Color.yellow);
        //if (Vector3.Distance(transform.position, pos) < distance)
        //position = Vector3.MoveTowards(transform.position, pos, 1) * distance / 2;
        return position;
    }

    /// <summary>
    /// Returns the position of the user's cursor in worldspace
    /// </summary>
    /// <returns>Position of the user's cursor in worldspace</returns>
    Vector3 PlaceStuff() {
        Vector3 pos = transform.forward * -100;
        Camera camera = GetComponentInChildren<Camera>();
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~(1 << 8))) {
            pos = hit.point;
        }
        pos = doNotTouchTerrain(pos, 2);
        // Check if behind player - and distable projector if it is
        Vector3 toOther = pos - transform.position;
        if (isBehind(toOther))
            projector.gameObject.SetActive(false);
        else if (!projector.gameObject.activeSelf)
            projector.gameObject.SetActive(true);
        //Debug.DrawLine(pos, new Vector3(pos.x, 5, pos.z), Color.blue);
        return pos;
    }

    /// <summary>
    /// Is this point behind the player
    /// Using castAngles (-1 to 1): 1 if they point in exactly the same direction, -1 if they point in completely opposite directions and 0 if the vectors are perpendicular.
    /// </summary>
    /// <param name="point">Point to be checked against</param>
    /// <returns>True if the point is in front of (and inside of the castAngles)</returns>
    bool isBehind(Vector3 point) {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 toOther = projector.transform.position - transform.position;
        return Vector3.Dot(forward.normalized, toOther.normalized) < castAngles;
    }

    /// <summary>
    /// Set the state of whether the user is placing or not (for logic with other classes)
    /// Also enables/disables the projector, projecting aim
    /// </summary>
    /// <param name="activate">State of activation (isPlacing)</param>
    void Activate(bool activate) {
        active = activate;
        projector.gameObject.SetActive(activate);
    }

    /// <summary>
    /// Load the aim and wait for input to throw BOOMnana
    /// Left clicking outside the valid area will cancel placement of the trap
    /// Right clicking will cancel placement right away
    /// </summary>
    public IEnumerator Boomy(ThrowBoomnana ability) {
        distance = ability.distance;
        Activate(true);
        projector.GetComponent<Projector>().material.mainTexture = Resources.Load("Images/AimPointer") as Texture;
        projector.GetComponent<Projector>().aspectRatio = ability.distance / 2;
        while (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
            yield return new WaitForFixedUpdate();
        if (Input.GetMouseButtonDown(0)) { // Sometimes I haz to press twice TT-TT
            if (isBehind(projector.transform.position - transform.position))
                ability.Cancel();
            else {
                Vector3 me = doNotTouchTerrain(transform.position);
                Ray ray = new Ray(me, (projector.transform.position - me).normalized);
                ability.Throw(ray.GetPoint(distance));
            //ability.Throw(Vector3.MoveTowards(transform.position, projector.transform.position, distance));
            }
        } else
            ability.Cancel();
        yield return new WaitForFixedUpdate(); // Makes sure you don't activate anything else when you click
        Activate(false);
    }

    /// <summary>
    /// Keeps the y from hitting the terrain (except in extreme conditions)
    /// </summary>
    /// <param name="pos">Current position of the object</param>
    /// <param name="distance">Y-distance from terrain</param>
    /// <returns>Position away from the terrain</returns>
    Vector3 doNotTouchTerrain(Vector3 pos, float distance = 2) {
        if (pos.y < 1)
            pos.y = 1;
        Vector3 hoverPos = pos;
        RaycastHit hit;
        if (Physics.Raycast(pos, -Vector3.up, out hit, Mathf.Infinity, ~(1 << 8))) {
            var distancetoground = hit.distance;
            var heightToAdd = transform.localScale.y * distance;
            hoverPos.y = pos.y - distancetoground + heightToAdd;
        }
        return hoverPos;
    }
}
