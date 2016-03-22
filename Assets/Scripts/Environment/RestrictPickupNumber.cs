using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class RestrictPickupNumber : NetworkBehaviour
{
    public PickupSpawner[] pickupsSpawner;
    public List<GameObject> pickupsGameObjects;

    // Use this for initialization
    void Start()
    {
        Random.seed = 42;
        pickupsSpawner = GetComponentsInChildren<PickupSpawner>();
        pickupsGameObjects = new List<GameObject>();
        foreach (PickupSpawner pickSpawn in pickupsSpawner)
        {
            pickupsGameObjects.Add(pickSpawn.gameObject);
        }
        ReducePickupNumber(4);
    }

    /// <summary>
    /// based on the amount you want to removed and not active.
    /// </summary>
    /// <param name="amount"></param>
    void ReducePickupNumber(int amount)
    {
        if (amount < pickupsSpawner.Length)
        {
            int value = Random.Range(0, pickupsSpawner.Length);
            List<GameObject> toBeRemovedList = pickupsGameObjects;
            for (int i = 0; i < amount; i++)
            {
                toBeRemovedList.RemoveAt(value);
                value = Random.Range(0, toBeRemovedList.Count);
            }
            print("toBeRemovedList Count = " + toBeRemovedList.Count);
            foreach (GameObject go in toBeRemovedList)
            {
                Destroy(go);
            }
        }
    }
}
