using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Box : NetworkBehaviour
{
    [SerializeField]
    GameObject banana;
    [SerializeField]
    GameObject stick;
    [SerializeField]
    GameObject sap;
    [SerializeField]
    GameObject leaf;
    [SerializeField]
    GameObject berry;

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {

    }

    IEnumerator BlowUpInFace(PlayerStats ps) {
        // TODO: Add Confetti and funzies (jack in the box!)
        int stuffz = 42;
        for (int i = 0; i < stuffz; i++) {
            GameObject go = null;
            int rnd = Random.Range(0, 5);
            switch (rnd) {
                case(0):
                    go = banana;
                    break;
                case (1):
                    go = stick;
                    break;
                case (2):
                    go = sap;
                    break;
                case (3):
                    go = leaf;
                    break;
                default:
                    go = berries(berry, ps);
                    break;
            }
            AreaPlacement(5, transform.position + transform.up * 3, go);
            yield return new WaitForSeconds(.5f);
        }
    }

    GameObject berries(GameObject berry, PlayerStats ps) {
        int dice = Random.Range(0, 2);
        string type = "";
        switch (dice) {
            case (0): type = "BerryG"; break;
            case (1): type = "BerryB"; break;
        }
        berry.GetComponent<Herb>().ChangeProperties(type, ps);
        berry.GetComponent<Pickup>().owner = ps.transform;
        return berry;
    }

    /// <summary>
    /// Based on a Radius, the method will spawn randomly placed assets within a Spherical area.
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="where">Where to place the spawner</param>
    /// <param name="asset"></param>
    PickupSpawner AreaPlacement(float radius, Vector3 where, GameObject asset) {
        PickupSpawner ps = null;
        #region UnityTerrain: If we are using a Unity Terrain.

    makeRay:
        Vector3 randomVector = Random.insideUnitSphere * radius * 3;
        randomVector.y = -radius * 3;
        Ray ray = new Ray(where, randomVector);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, float.MaxValue, (1 << 9) | (1 << 0) | (1 << 10) | (1 << 11))) {
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Default") ||
                /*hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Environment") ||*/
                hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Trees")) {
                goto makeRay;
            }
            else {
                ps = CreateObject(asset, hitInfo.point + Vector3.up * 1.25f, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));

                Debug.DrawLine(where, hitInfo.point, Color.red, 100f);
            }
        }
        return ps;
        #endregion
    }

    PickupSpawner CreateObject(GameObject go, Vector3 p, Quaternion r) {
        var spawner = (GameObject)GameObject.Instantiate(go, p, r);
        NetworkServer.Spawn(spawner);
        return spawner.GetComponent<PickupSpawner>();
    }

    void OnTriggerEnter(Collider _col) {
        if (!isServer)
            return;
        PlayerStats ps = _col.GetComponent<PlayerStats>();
        if (ps) {
            BlowUpInFace(ps);
            Destroy(gameObject, 5); // spawn lidless - giggity
        } 
    }
}
