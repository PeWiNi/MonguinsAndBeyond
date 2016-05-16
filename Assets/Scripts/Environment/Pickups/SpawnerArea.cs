using UnityEngine;
using System.Collections;

public class SpawnerArea : MonoBehaviour {
    public SpawnerType Spawner1;
    public SpawnerType Spawner2;
    public SpawnerType Spawner3;

    [SerializeField]
    Transform Spawner1Location;
    [SerializeField]
    Transform Spawner2Location;
    [SerializeField]
    Transform Spawner3Location;

    public Vector3 Spawner1Pos { get { return Spawner1Location.position; } }
    public Vector3 Spawner2Pos { get { return Spawner2Location.position; } }
    public Vector3 Spawner3Pos { get { return Spawner3Location.position; } }

    public int Spawner1Count;
    public int Spawner2Count;
    public int Spawner3Count;

    public enum SpawnerType {
        Banana, Stick, Sap, Leaf, Berry, Fish, Unknown
    }

    SpawnerType determineType(string s) {
        switch (s) {
            case "Banana":
                return SpawnerType.Banana;
            case "Stick":
                return SpawnerType.Stick;
            case "Sap":
                return SpawnerType.Sap;
            case "Leaf":
                return SpawnerType.Leaf;
            case "Berry":
                return SpawnerType.Berry;
            case "Fish":
                return SpawnerType.Fish;
            default:
                return SpawnerType.Unknown;
        }
    }
}
