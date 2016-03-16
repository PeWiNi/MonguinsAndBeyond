﻿using UnityEngine;
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
    public Condition conditionState = Condition.None;//Default None.
    [SyncVar]
    string mat;
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
        transform.FindChild("Berry").GetComponent<MeshRenderer>().material = Resources.Load(mat) as Material;
    }

    public void ChangeProperties(string type) {
        mat = "Materials/Herb" + (type == "BerryG" ? " Good" : type == "BerryB" ? " Bad" : "");
        conditionState = type == "BerryG" ? Condition.Regeneration :
            type == "BerryB" ? Condition.Degenration : Condition.Random;
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

    void OnTriggerEnter(Collider _collider) {
        if(_collider.tag == "Player") {
            PlayerStats ps = _collider.gameObject.GetComponent<PlayerStats>();
            if (conditionState == Condition.None)
                ps.GetComponent<SyncInventory>().pickupBerry(Random.Range(0, 100)); // Why they always pick up new regardless?
            else if (conditionState == Condition.Regeneration)
                ps.GoodBerry(amount, duration);
            else if (conditionState == Condition.Degenration)
                ps.BadBerry(amount, duration);
            else //if (conditionState == Condition.Random)
                RandomCondition(ps);
            Destroy(gameObject);
        }
    }
}
