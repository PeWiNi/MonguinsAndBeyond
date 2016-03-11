using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SpawnTraps : NetworkBehaviour {
    // ADD PREVIEWS of traps
    // Interconnectivity with CharacterCamera (disable while putting traps so you can properly place the trap)
    string bananaTrap;

    GameObject projector;

    bool active = false;
    public bool Placing {
        get {
            return active;
        }
    }

	// Use this for initialization
	void Start () {
        projector = (GameObject)Instantiate(Resources.Load("Prefabs/Trap_Projector") as GameObject, transform.position,
            Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));
        bananaTrap = "Prefabs/Trap_BananaIsland";
    }
	
	// Update is called once per frame
	void Update () {
	    if (Placing) {
            projector.transform.position = PlaceStuff();
        }
	}

    Vector3 PlaceStuff() {
        Vector3 pos = new Vector3();
        Camera camera = GetComponentInChildren<Camera>();
        //pos = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camera.nearClipPlane * camera.GetComponent<CharacterCamera>().currentDistance));
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~(1 << 8))) {
            pos = hit.point;
        }
        pos = Limiter(doNotTouchTerrain(pos, 2));
        Debug.DrawLine(pos, new Vector3(pos.x, 5, pos.z), Color.blue);
        return pos;
    }

    Vector3 Limiter(Vector3 pos) {
        return pos;
    }

    void Activate(bool activate) {
        active = activate;
        projector.gameObject.SetActive(activate);
    }

    public IEnumerator Slippery() {
        Activate(true);
        projector.GetComponent<Projector>().material.mainTexture = Resources.Load("Images/BananaSplat_Decal") as Texture;
        while (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
            yield return new WaitForFixedUpdate();
        if(Input.GetMouseButtonDown(0))
            CmdDoFire(bananaTrap, projector.transform.position);
        else 
            GetComponent<SyncInventory>().pickupBanana(); // Mayhaps this requires a Command...
        Activate(false);
    }

    [Command]
    void CmdDoFire(string go, Vector3 position) {
        Vector3 spawnPos = transform.position + ((transform.localScale.x + 10) * transform.forward);
        if (position != new Vector3())
            spawnPos = position;
        spawnPos = doNotTouchTerrain(spawnPos, 2);
        GameObject banana = (GameObject)Instantiate(
            Resources.Load(go) as GameObject, spawnPos,
            Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));

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
