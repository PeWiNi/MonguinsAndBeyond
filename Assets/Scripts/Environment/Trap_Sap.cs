using UnityEngine;
using UnityEngine.Networking;

public class Trap_Sap : NetworkBehaviour {

    void OnTriggerEnter(Collider _col) {
        if (_col.tag == "Player") {
            _col.transform.GetComponent<PlayerStats>().Slow(true);
            SpawnDripper(_col.transform);
        }
    }

    void OnTriggerStay(Collider _col) {
        if (_col.tag == "Player") {
            _col.transform.GetComponent<PlayerStats>().Slow(true);
        }
    }
    //void OnDestroy() { }
    
    void SpawnDripper(Transform trans) {
        if (!isServer)
            return;
        GameObject dripper = (GameObject)Instantiate(
            Resources.Load("Prefabs/Environments/ParticleSystems/Dripping") 
            as GameObject, trans.position, Quaternion.identity);

        dripper.GetComponent<DripScript>().SetParent(trans);

        NetworkServer.Spawn(dripper);
    }
}
