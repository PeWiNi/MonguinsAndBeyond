using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SpawnTraps : NetworkBehaviour {
    // Interconnectivity with CharacterCamera (disable while putting traps so you can properly place the trap)
    // THOUGHT: Accumulate traps so you can throw multiple banana-peels at the same time (not on top of itself)
    string bananaTrap;
    string spikeTrap;

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
        projector = (GameObject)Instantiate(Resources.Load("Prefabs/Trap_Projector") as GameObject, transform.position,
            Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));
        bananaTrap = "Prefabs/Trap_BananaIsland";
        spikeTrap = "Prefabs/Trap_SpikeIsland";
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
        pos = doNotTouchTerrain(Limiter(pos, range), 2);
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

    /// <summary>
    /// Set the state of whether the user is placing or not (for logic with other classes)
    /// Also enables/disables the projector, projecting the trap placement
    /// </summary>
    /// <param name="activate">State of activation (isPlacing)</param>
    void Activate(bool activate) {
        active = activate;
        projector.gameObject.SetActive(activate);
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
        Activate(false);
    }

    public IEnumerator Spikey() {
        Activate(true);
        projector.GetComponent<Projector>().material.mainTexture = Resources.Load("Images/Spikes") as Texture;
        while (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
            yield return new WaitForFixedUpdate();
        if (Input.GetMouseButtonDown(0)) {
            if (isBehind(projector.transform.position - transform.position))
                GetComponent<SyncInventory>().pickupSticks();
            else CmdDoFire(spikeTrap, projector.transform.position, spikeTrapDuration);
        } else
            GetComponent<SyncInventory>().pickupSticks(); // Mayhaps this requires a Command...
        Activate(false);
    }

    /// <summary>
    /// Spawn object on server
    /// </summary>
    /// <param name="go">String to position of GameObject in Resources</param>
    /// <param name="position">The position at which the object is to be spawned</param>
    /// <param name="duration">Time before object is destoryed (0 means never)</param>
    [Command]
    void CmdDoFire(string go, Vector3 position, float duration) {
        Vector3 spawnPos = transform.position + ((transform.localScale.x + 10) * transform.forward);
        if (position != new Vector3())
            spawnPos = position;
        spawnPos = doNotTouchTerrain(spawnPos, 2);
        GameObject banana = (GameObject)Instantiate(
            Resources.Load(go) as GameObject, spawnPos,
            Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));

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
