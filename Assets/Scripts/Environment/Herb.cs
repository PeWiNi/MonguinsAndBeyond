using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Herb : Pickup {
    public enum Condition {
        Regeneration,
        Degenration,
        Random,
        None
    };

    [SyncVar]
    int teamOwner;
    [SyncVar]
    public Condition conditionState = Condition.None;//Default None.
    [SyncVar]
    string mat = "Materials/berry_neutral";
    [Range(0f, 20f)]
    public float duration = 10f;
    [Range(0f, 50f)]
    public float amount = 50f;
    int value;
    public int randomValue {
        get { return value; }
    }

    // Use this for initialization
    void Start() {
        try { if (!isServer) {
                PlayerStats ps = GameObject.Find("NetworkManager").GetComponentInChildren<HUDScript>().GetPlayerStats();
                if (ps.team != teamOwner)
                    transform.FindChild("Berry").GetComponent<MeshRenderer>().material = Resources.Load("Materials/berry_neutral") as Material;
                else
                    transform.FindChild("Berry").GetComponent<MeshRenderer>().material = Resources.Load(mat) as Material;
            }
        } catch { transform.FindChild("Berry").GetComponent<MeshRenderer>().material = Resources.Load(mat) as Material; }
        transform.position = doNotTouchTerrain(transform.position);
    }

    public void ChangeProperties(string type, int team) {
        mat = "Materials/berry" + (type == "BerryG" ? "_good" : type == "BerryB" ? "_bad" : "_neutral");
        conditionState = type == "BerryG" ? Condition.Regeneration :
            type == "BerryB" ? Condition.Degenration : Condition.Random;
        teamOwner = team;
    }

    /// <summary>
    /// Receive a psuedo random condition.
    /// </summary>
    public void RandomCondition(PlayerStats ps) {
        System.Array values = Condition.GetValues(typeof(Condition));
        System.Random random = new System.Random();
        conditionState = (Condition)values.GetValue(random.Next(values.Length));
        while (conditionState == Condition.None || conditionState == Condition.Random) {
            conditionState = (Condition)values.GetValue(random.Next(values.Length));
        }
        if (conditionState == Condition.Regeneration)
            ps.GoodBerry(amount, duration);
        if (conditionState == Condition.Degenration)
            ps.BadBerry(amount, duration);
    }

    public void EatIt(PlayerStats ps) {
        if (conditionState == Condition.Regeneration)
            ps.GoodBerry(amount, duration);
        else if (conditionState == Condition.Degenration)
            ps.BadBerry(amount, duration);
        else //if (conditionState == Condition.Random)
            RandomCondition(ps);
    }

    void OnTriggerEnter(Collider _collider) {
        if (!canCollide)
            return;
        if (_collider.tag == "Player") {
            PlayerStats ps = _collider.gameObject.GetComponent<PlayerStats>();
            if (conditionState == Condition.None)
                ps.GetComponent<SyncInventory>().pickupBerry(Random.Range(0, 100));
            else if (_collider.GetComponent<PlayerStats>().team == owner.GetComponent<PlayerStats>().team)
                ps.GetComponent<SyncInventory>().pickupBerry(conditionState);
            else if (conditionState == Condition.Regeneration)
                ps.GoodBerry(amount, duration);
            else if (conditionState == Condition.Degenration)
                ps.BadBerry(amount, duration);
            else //if (conditionState == Condition.Random)
                RandomCondition(ps);

            if (isServer) {
                GameObject particles = (GameObject)Instantiate(
                Resources.Load("Prefabs/Environments/ParticleSystems/BerryPS"),
                    transform.position, Quaternion.Euler(270f, 0, 0));
                Destroy(particles, 5);
                NetworkServer.Spawn(particles);
            }

            Destroy(gameObject);
        }
    }
}
