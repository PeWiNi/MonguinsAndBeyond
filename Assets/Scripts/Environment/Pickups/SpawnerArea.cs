using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class SpawnerArea : NetworkBehaviour {
    SpawnerType Spawner1;
    SpawnerType Spawner2;
    SpawnerType Spawner3;

    List<PickupSpawner> Spawners1;
    List<PickupSpawner> Spawners2;
    List<PickupSpawner> Spawners3;

    [SerializeField]
    Transform Spawner1Location;
    [SerializeField]
    Transform Spawner2Location;
    [SerializeField]
    Transform Spawner3Location;

    public Vector3 Spawner1Pos { get { return Spawner1Location.position; } }
    public Vector3 Spawner2Pos { get { return Spawner2Location.position; } }
    public Vector3 Spawner3Pos { get { return Spawner3Location.position; } }

    public int Capacity = 5;

    // TODO: Figure out a smart way to spawn asset signifying the type of collectable spawned in that area
    [SerializeField]
    EnvironmentPlacement BananaSignifier;
    [SerializeField]
    EnvironmentPlacement StickSignifier;
    [SerializeField]
    EnvironmentPlacement SapSignifier;
    [SerializeField]
    EnvironmentPlacement LeafSignifier;
    [SerializeField]
    EnvironmentPlacement BerrySignifier;

    public GameObject terrainParent = null;
    TerrainInfo terrainParentInfo;

    public enum SpawnerType {
        Unknown, Banana, Stick, Sap, Leaf, Berry, Fish
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

    void Start() {
        if (terrainParent != null)
            terrainParentInfo = terrainParent.GetComponent<TerrainInfo>();
    }

    public void AssignSpawnerType(string type) { AssignSpawnerType(determineType(type)); }
    void AssignSpawnerType(SpawnerType type) {
        if (ContainsType(type) == 1) 
            Debug.Log(type.ToString() + " already exists in " + name);
        else if (Spawner1 == SpawnerType.Unknown) {
            Spawner1 = type;
            Spawners1 = new List<PickupSpawner>();
            setupTypeProps(type);
        } else if (Spawner2 == SpawnerType.Unknown) {
            Spawner2 = type;
            Spawners2 = new List<PickupSpawner>();
            setupTypeProps(type);
        } else if (Spawner3 == SpawnerType.Unknown) {
            Spawner3 = type;
            Spawners3 = new List<PickupSpawner>();
            setupTypeProps(type);
        } else {
            Debug.Log("Cannot add " + type.ToString() + "; no more room in " + name);
        }
    }

    /// <summary>
    /// Setup <fern, bush, bananaTree, etc.>
    /// </summary>
    /// <param name="type"></param>
    void setupTypeProps(SpawnerType type) {
        switch(type) {
            case (SpawnerType.Banana):
                break;
            case (SpawnerType.Stick):
                break;
            case (SpawnerType.Sap):
                break;
            case (SpawnerType.Leaf):
                break;
            case (SpawnerType.Berry):
                break;
            default:
                break;
        }
    }

    public void AddSpawner(string type, GameObject spawner) { AddSpawner(determineType(type), spawner); }
    void AddSpawner(SpawnerType type, GameObject spawner) {
        if (ContainsType(type) == 0)
            Debug.Log("There are no " + type.ToString() + " here in " + name);
            
        else if (ContainsType(type) == 1) {
            Vector3 randomVector = Random.insideUnitSphere * 5;
            randomVector.y = 5f;
            if (type == Spawner1) {
                Spawners1.Add(AreaPlacement(5, Spawner1Pos + randomVector, spawner, Spawner1Location));
            } else if (type == Spawner2) {
                Spawners2.Add(AreaPlacement(5, Spawner2Pos + randomVector, spawner, Spawner2Location));
            } else if (type == Spawner3) {
                Spawners3.Add(AreaPlacement(5, Spawner3Pos + randomVector, spawner, Spawner3Location));
            } 
        }
    }

    public void RemoveSpawner(string type) { RemoveSpawner(determineType(type)); }
    void RemoveSpawner(SpawnerType type) {
        if (ContainsType(type) == 0)
            Debug.Log("There are no " + type.ToString() + " here in " + name);
        else if (type == Spawner3) {
            if (Spawners3.Count > 0) {
                Destroy(Spawners3[Spawners3.Count - 1].gameObject);
                Spawners3.RemoveAt(Spawners3.Count - 1);
            }
            if (Spawners3.Count == 0) {
                Spawner3 = SpawnerType.Unknown;
                Spawners3 = null;
                // Remove all children (clean slate)
                var children = new List<GameObject>();
                foreach (Transform child in Spawner3Location) children.Add(child.gameObject);
                children.ForEach(child => Destroy(child));
            }
        } else if (type == Spawner2) {
            if (Spawners2.Count > 0) {
                Destroy(Spawners2[Spawners2.Count - 1].gameObject);
                Spawners2.RemoveAt(Spawners2.Count - 1);
            }
            if (Spawners2.Count == 0) {
                Spawner2 = SpawnerType.Unknown;
                Spawners2 = null;
                // Remove all children (clean slate)
                var children = new List<GameObject>();
                foreach (Transform child in Spawner2Location) children.Add(child.gameObject);
                children.ForEach(child => Destroy(child));
            }
        } else if (type == Spawner1) {
            if (Spawners1.Count > 0) {
                Destroy(Spawners1[Spawners1.Count - 1].gameObject);
                Spawners1.RemoveAt(Spawners1.Count - 1);
            }
            if (Spawners1.Count == 0) {
                Spawner1 = SpawnerType.Unknown;
                Spawners1 = null;
                // Remove all children (clean slate)
                var children = new List<GameObject>();
                foreach (Transform child in Spawner1Location) children.Add(child.gameObject);
                children.ForEach(child => Destroy(child));
            }
        } 
    }

    public int CountSpawners(string type) { return CountSpawners(determineType(type)); }
    int CountSpawners(SpawnerType type) {
        if (ContainsType(type) == 0)
            Debug.Log("No instances of " + type.ToString() + " found in " + name);
        else if (Spawner1 == type) {
            if (Spawners1.Count > 1) Spawners1.RemoveAll(x => x == null);
            return Spawners1.Count;
        } else if (Spawner2 == type) {
            if (Spawners2.Count > 1) Spawners2.RemoveAll(x => x == null);
            return Spawners2.Count;
        } else if (Spawner3 == type) {
            if (Spawners3.Count > 1) Spawners3.RemoveAll(x => x == null);
            return Spawners3.Count;
        }
        return 0;
    }

    #region Area Type check methods
    int ContainsType(SpawnerType type) {
        if (type == Spawner1) {
            return Spawn1(type);
        } else if (type == Spawner2) {
            return Spawn2(type);
        } else if (type == Spawner3) {
            return Spawn3(type);
        }
        return 0;
    }

    public int SpawnIteration(string type, int area = 0) { return SpawnIteration(determineType(type), area); }
    int SpawnIteration(SpawnerType type, int area) {
        switch (area) {
            case (1):
                return Spawn1(type);
            case (2):
                return Spawn2(type);
            case (3):
                return Spawn3(type);
            default:
                return ContainsType(type);
        }
    }
    int Spawn1(SpawnerType type) {
        if (type == Spawner1) { 
            if (Spawners1.Count < Capacity)
                return 1;
            return 2;
        }
        return 0;
    }
    int Spawn2(SpawnerType type) {
        if (type == Spawner2) { 
            if (Spawners2.Count < Capacity)
                return 1;
            return 2;
        }
        return 0;
    }
    int Spawn3(SpawnerType type) {
        if (type == Spawner3) {
            if (Spawners3.Count < Capacity)
                return 1;
            return 2;
        }
        return 0;
    }
    #endregion

    /// <summary>
    /// Based on a Radius, the method will spawn randomly placed assets within a Spherical area.
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="where">Where to place the spawner</param>
    /// <param name="asset"></param>
    PickupSpawner AreaPlacement(float radius, Vector3 where, GameObject asset, Transform parent) {
        PickupSpawner ps = null;
        #region UnityTerrain: If we are using a Unity Terrain.
        TerrainData myTerrainData = terrainParentInfo.GetTerrainData();

        makeRay:
        Vector3 randomVector = Random.insideUnitSphere * radius * 3;
        randomVector.y = -radius * 3;
        Ray ray = new Ray(where, randomVector);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, float.MaxValue, (1 << 9) | (1 << 0) | (1 << 10) | (1 << 11))) {
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Default") ||
                hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Environment") ||
                hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Trees")) {
                goto makeRay;
            } else {
                ps = CreateObject(asset, hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal), parent.gameObject);

                Debug.DrawLine(where, hitInfo.point, Color.red, 100f);
            }
        }
        return ps;
        #endregion
    }

    PickupSpawner CreateObject(GameObject go, Vector3 p, Quaternion r, GameObject g) {
        var spawner = (GameObject)GameObject.Instantiate(go, p, r);
        spawner.transform.SetParent(g.transform);
        NetworkServer.Spawn(spawner);
        return spawner.GetComponent<PickupSpawner>();
    }
}
