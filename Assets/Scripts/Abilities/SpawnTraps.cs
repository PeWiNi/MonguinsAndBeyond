using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SpawnTraps : NetworkBehaviour {
    // ADD PREVIEWS of traps
    // Interconnectivity with CharacterCamera (disable while putting traps so you can properly place the trap)
    string bananaTrap;

	// Use this for initialization
	void Start () {
        bananaTrap = "Prefabs/Trap_BananaIsland";
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Slippery() {
        Vector3 spawnPos = transform.position + ((transform.localScale.x + 10) * transform.forward);
        spawnPos.y = 2;// = doNotTouchTerrain(spawnPos, 2);
        Quaternion spawnAngle = Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
        CmdDoFire(bananaTrap, new Vector3(spawnPos.x, spawnPos.y, spawnPos.z), Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));
    }

    [Command]
    void CmdDoFire(string go, Vector3 position, Quaternion angle) {
        Vector3 spawnPos = transform.position + ((transform.localScale.x + 10) * transform.forward);
        spawnPos.y = 2;// = doNotTouchTerrain(spawnPos, 2);
        GameObject banana = (GameObject)Instantiate(
            Resources.Load(go) as GameObject, spawnPos, //position, angle);
            Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));

        NetworkServer.Spawn(banana);
    }

    /// <summary>
    /// Keeps the y from hitting the terrain (except in extreme conditions)
    /// </summary>
    /// <param name="pos">Current position of the object</param>
    /// <param name="distance">Y-distance from terrain</param>
    /// <returns>Position away from the terrain</returns>
    Vector3 doNotTouchTerrain(Vector3 pos, float distance) {
        Vector3 hoverPos = pos;
        RaycastHit hit;
        if (Physics.Raycast(pos, -Vector3.up, out hit, ~(1 << 8))) {
            var distancetoground = hit.distance;
            var heightToAdd = transform.localScale.y * distance;
            hoverPos.y = pos.y - distancetoground + heightToAdd;
        }
        return hoverPos;
    }
}
