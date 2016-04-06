using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SpawnTraps : NetworkBehaviour {
    // Interconnectivity with CharacterCamera (disable while putting traps so you can properly place the trap)
    // THOUGHT: Accumulate traps so you can throw multiple banana-peels at the same time (not on top of itself)
    public string bananaTrap;
    public string spikeTrap;
    string sap;
    [SyncVar]
    float distFromTerrain = 2;
    GameObject projector;

    bool active = false;
    public bool isPlacing {
        get {
            return active;
        }
    }
    public float range = 10f;
    public float playerOffset = 1.5f;
    [Range(0, 1)]
    public float castAngles = 0f;

    public float bananaTrapDuration = 60f; // Update to use the Trap class duration
    public float spikeTrapDuration = 120f; // Update to use the Trap class duration

    // Use this for initialization
    void Start () {
        bananaTrap = "Prefabs/Environments/Trap_BananaIsland";
        spikeTrap = "Prefabs/Environments/Trap_SpikeIsland";
        sap = "Prefabs/Environments/Trap_Sap";
    }
	
	// Update is called once per frame
	void Update () {
	    if (isPlacing) {
            projector.transform.position = PlaceStuff();
        }
	}

    /// <summary>
    /// Returns the position of the user's cursor in worldspace possibly limited by Limiter()
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
        pos = doNotTouchTerrain(Limiter(pos, range), distFromTerrain);
        // Check if behind player - and disable projector if it is
        Vector3 toOther = pos - transform.position;
        if (isBehind(toOther) || tooFarAway(pos))
            projector.gameObject.SetActive(false);
        else if (!projector.gameObject.activeSelf)
            projector.gameObject.SetActive(true);
        //Debug.DrawLine(pos, new Vector3(pos.x, 5, pos.z), Color.blue);
        return pos;
    }

    /// <summary>
    /// Limit the space in which the Trap can be placed
    /// </summary>
    /// <param name="pos">The position that is to be constrained</param>
    /// <param name="distance">The distance from the player that the trap can be set</param>
    /// <returns>The position Clamped inside the available space</returns>
    Vector3 Limiter(Vector3 pos, float distance) {
        Vector3 position = pos;
        // Check if within distance
        if (Vector3.Distance(transform.position, pos) > distance)
            position = Vector3.MoveTowards(transform.position, pos, distance);
        return position;
    }

    /// <summary>
    /// Is this point behind the player (+ offset)
    /// Using castAngles (-1 to 1): 1 if they point in exactly the same direction, -1 if they point in completely opposite directions and 0 if the vectors are perpendicular.
    /// </summary>
    /// <param name="point">Point to be checked against</param>
    /// <returns>True if the point is in front of (and inside of the castAngles)</returns>
    bool isBehind(Vector3 point) {
        Vector3 forward = transform.TransformDirection(Vector3.forward) + transform.forward * playerOffset;
        Vector3 toOther = projector.transform.position - transform.position;
        return Vector3.Dot(forward.normalized, toOther.normalized) < castAngles;
    }

    bool tooFarAway(Vector3 point) {
        return Vector3.Distance(transform.position, point) > (range * 2);
    }

    /// <summary>
    /// Set the state of whether the user is placing or not (for logic with other classes)
    /// Also enables/disables the projector, projecting the trap placement
    /// </summary>
    /// <param name="activate">State of activation (isPlacing)</param>
    void Activate(bool activate, bool threeDee = false) {
        if(activate) {
            if (threeDee) {
                projector = (GameObject)Instantiate(Resources.Load("Prefabs/Environments/Trap_Projector_3D") as GameObject, transform.position,
                    Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));
                distFromTerrain = 0;
            } else {
                projector = (GameObject)Instantiate(Resources.Load("Prefabs/Environments/Trap_Projector") as GameObject, transform.position,
                    Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));
                distFromTerrain = 2;
            }
            projector.gameObject.SetActive(activate);
            CmdChangeDist(distFromTerrain);
        } else { Destroy(projector); }
        active = activate;
    }

    [Command]
    void CmdChangeDist(float dist) {
        distFromTerrain = dist;
    }

    /// <summary>
    /// Spawn the bananaTrap and wait for input (from the user on where to place it)
    /// Left clicking outside the valid area will cancel placement of the trap
    /// Right clicking will cancel placement right away
    /// </summary>
    public IEnumerator Slippery() {
        Activate(true);
        projector.GetComponent<Projector>().material.mainTexture = Resources.Load("Images/BananaSplat_Decal") as Texture;
        while (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
            yield return new WaitForFixedUpdate();
        if(Input.GetMouseButtonDown(0)) {
            if (isBehind(projector.transform.position - transform.position))
                GetComponent<SyncInventory>().pickupBanana();
            else CmdDoFire(bananaTrap, projector.transform.position, bananaTrapDuration);
        }
        else 
            GetComponent<SyncInventory>().pickupBanana(); // Mayhaps this requires a Command...
        yield return new WaitForFixedUpdate();
        Activate(false);
    }

    public IEnumerator Spikey() {
        Activate(true, true);
        //projector.GetComponent<Projector>().material.mainTexture = Resources.Load("Images/Spikes") as Texture;
        while (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
            yield return new WaitForFixedUpdate();
        if (Input.GetMouseButtonDown(0)) {
            if (isBehind(projector.transform.position - transform.position)) {
                GetComponent<SyncInventory>().pickupSticks();
                GetComponent<SyncInventory>().pickupLeaf();
            }
            else CmdDoFire(spikeTrap, projector.transform.position, spikeTrapDuration);
        } else { // Mayhaps this requires a Command...
            GetComponent<SyncInventory>().pickupSticks();
            GetComponent<SyncInventory>().pickupLeaf();
        }
        Activate(false);
    }

    /// <summary>
    /// Throw sap
    /// TODO: Check for spike trap (for combining) and water (for hardening) 
    /// </summary>
    /// <returns></returns>
    public IEnumerator StickySap() {
        Activate(true, true);
        //projector.GetComponent<Projector>().material.mainTexture = Resources.Load("Images/xMarksTheSpot") as Texture;
        while (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
            yield return new WaitForFixedUpdate();
        if (Input.GetMouseButtonDown(0)) {
            if (isBehind(projector.transform.position - transform.position))
                GetComponent<SyncInventory>().pickupSap();
            else CmdDoFire(sap, projector.transform.position, 0);
        } else
            GetComponent<SyncInventory>().pickupSap(); // Mayhaps this requires a Command...
        Activate(false);
    }

    /// <summary>
    /// Spawn object on server
    /// </summary>
    /// <param name="go">String to position of GameObject in Resources</param>
    /// <param name="position">The position at which the object is to be spawned</param>
    /// <param name="duration">Time before object is destoryed (0 means never)</param>
    [Command]
    public void CmdDoFire(string go, Vector3 position, float duration) {
        Vector3 spawnPos = transform.position + ((transform.localScale.x + 10) * transform.forward);
        if (position != new Vector3())
            spawnPos = position;
        spawnPos = doNotTouchTerrain(spawnPos, distFromTerrain);
        GameObject banana = (GameObject)Instantiate(
            Resources.Load(go) as GameObject, spawnPos,
            Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));

        banana.GetComponent<Trap>().SetOwner(gameObject);

        if (duration > 0)
            Destroy(banana, duration);

        NetworkServer.Spawn(banana);
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
