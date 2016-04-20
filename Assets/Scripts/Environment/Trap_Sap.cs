using UnityEngine;
using UnityEngine.Networking;

public class Trap_Sap : Trap {
    float slowTime = 1f;

    void OnTriggerEnter(Collider _col) {
        if (_col.tag == "Player") {
            if(!_col.GetComponent<PlayerStats>().isSlowed) SpawnDripper(_col.transform);
            _col.transform.GetComponent<PlayerStats>().Slow(true, slowTime * _col.transform.GetComponent<PlayerStats>().sapModifier);
        }
    }

    void OnTriggerStay(Collider _col) {
        if (_col.tag == "Player") {
            _col.transform.GetComponent<PlayerStats>().Slow(true, slowTime * _col.transform.GetComponent<PlayerStats>().sapModifier);
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
