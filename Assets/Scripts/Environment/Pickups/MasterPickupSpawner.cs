using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class MasterPickupSpawner : NetworkBehaviour {
	private MyNetworkManager manager;

    [SerializeField]
    public bool showGUI = true;
    [SerializeField]
    public int offsetX;
    [SerializeField]
    public int offsetY;

    [Tooltip("Required for PlacementState \nThis value will be the radius around the 'startingPoint' to spawn GameObjects in assets")]
    public int areaRadius = 25;

    [Tooltip("Required for PlacementState \nDefault (True)")]
    public bool isUsingUnityTerrain = true;
    public GameObject terrainParent = null;
    TerrainInfo terrainParentInfo;

    [SerializeField] GameObject BananaSpawner;
    [SerializeField] GameObject StickSpawner;
    [SerializeField] GameObject SapSpawner;
    [SerializeField] GameObject LeafSpawner;
    [SerializeField] GameObject BerrySpawner;
    [SerializeField] GameObject FishSpawner;

    List<PickupSpawner> bananaSpawners = new List<PickupSpawner>();
    List<PickupSpawner> stickSpawners = new List<PickupSpawner>();
    List<PickupSpawner> sapSpawners = new List<PickupSpawner>();
    List<PickupSpawner> leafSpawners = new List<PickupSpawner>();
    List<PickupSpawner> berrySpawners = new List<PickupSpawner>();
    List<PickupSpawner> fishSpawners = new List<PickupSpawner>();

    float bananaTimer;
    float stickTimer;
    float sapTimer;
    float leafTimer;
    float berryTimer;

    int[] players = new int[3];

    SpawnerArea[] collectableAreas;

    public float defaultSpawnValue = 1;
    public float maxSpawnValue = 5;
    public float defaultSpawnTime = 30;
    public float minSpawnTime = 15;

    // Use this for initialization
    void Start () {
        if (terrainParent != null)
            terrainParentInfo = terrainParent.GetComponent<TerrainInfo>();
        manager = GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>();
        StartCoroutine(GetCollectableAreas());
    }

    void Update() {
        if (!isServer) return;
        if (Input.GetKeyDown(KeyCode.U)) {
            Vector3 randomVector = Random.insideUnitSphere * 5;
            randomVector.y = 5f;
            FishPlacement(5, fishSpawners[Random.Range(0, fishSpawners.Count)].transform.position + randomVector, FishSpawner);
        }
        if(Input.GetKeyDown(KeyCode.B)) {
            print("Balance Time!");
            DetermineSpawnerValues(FindObjectsOfType<PlayerStats>());
        }
    }

    #region Event handling
    void OnEnable() {
        manager = GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>();
        manager.EventConnectPlayer += Listener;
        manager.EventDisconnectPlayer += Listener;
    }

    // Just to make sure that we unsubscribe when the object is no longer in use
    void OnDisable() {
        manager.EventConnectPlayer -= Listener;
        manager.EventDisconnectPlayer -= Listener;
    }

    void Listener() {
        DetermineSpawnerValues(FindObjectsOfType<PlayerStats>(), true);
    }
    #endregion

    void OnGUI() {
        if (!isServer || !showGUI) return;

        int xpos = 10 + offsetX;
        int ypos = 10 + offsetY;
        int spacing = 24;

        if (NetworkServer.active) {
            GUI.Label(new Rect(xpos, ypos, 300, 20), "Server: address=" + manager.networkAddress + " port=" + manager.networkPort);
            ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Use Collectables Area Code")) {
                DetermineSpawnerValues(FindObjectsOfType<PlayerStats>(), true);
            }
            ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Rebalance Spawners")) {
                DetermineSpawnerValues(FindObjectsOfType<PlayerStats>());
                StartCoroutine(GetSpawners());
            }
            int playersSum = players[0] + players[1] + players[2];
            ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 500, 20), string.Format("Total # of players: {0}. Defenders: {1}, Attackers: {2}, Supporters: {3}", playersSum, players[1], players[0], players[2]))) { print("It's not very effective!"); }
            #region Cheat in Players
            if (GUI.Button(new Rect(xpos + 500, ypos, 120, 20), string.Format("Defenders: {0} +1", players[1]))) {
                StartCoroutine(spawnerMasterLogic(players[0], ++players[1], players[2], new float[] { 1.5f, 1.3f, 1.2f, 1.0f }, players[0] + players[1] + players[2], true));
            } if (GUI.Button(new Rect(xpos + 620, ypos, 20, 20), "-1")) {
                StartCoroutine(spawnerMasterLogic(players[0], --players[1], players[2], new float[] { 1.5f, 1.3f, 1.2f, 1.0f }, players[0] + players[1] + players[2], true));
            } if (GUI.Button(new Rect(xpos + 500, ypos + spacing, 120, 20), string.Format("Attackers: {0} +1", players[0]))) {
                StartCoroutine(spawnerMasterLogic(++players[0], players[1], players[2], new float[] { 1.5f, 1.3f, 1.2f, 1.0f }, players[0] + players[1] + players[2], true));
            } if (GUI.Button(new Rect(xpos + 620, ypos + spacing, 20, 20), "-1")) {
                StartCoroutine(spawnerMasterLogic(--players[0], players[1], players[2], new float[] { 1.5f, 1.3f, 1.2f, 1.0f }, players[0] + players[1] + players[2], true));
            } if (GUI.Button(new Rect(xpos + 500, ypos + spacing + spacing, 120, 20), string.Format("Supporters: {0} +1", players[2]))) {
                StartCoroutine(spawnerMasterLogic(players[0], players[1], ++players[2], new float[] { 1.5f, 1.3f, 1.2f, 1.0f }, players[0] + players[1] + players[2], true));
            } if (GUI.Button(new Rect(xpos + 620, ypos + spacing + spacing, 20, 20), "-1")) {
                StartCoroutine(spawnerMasterLogic(players[0], players[1], --players[2], new float[] { 1.5f, 1.3f, 1.2f, 1.0f }, players[0] + players[1] + players[2], true));
            }
            #endregion
            ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 300, 20), string.Format("{0} Spawners: {1}, Timer: {2}", "Banana", bananaSpawners.Count, bananaTimer))) {
                //Vector3 randomVector = Random.insideUnitSphere * 5;
                //randomVector.y = 5f;
                //AreaPlacement(5, bananaSpawners[Random.Range(0, bananaSpawners.Count)].transform.position + randomVector, BananaSpawner);
                AddTypeToAreas("Banana");
            } ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 300, 20), string.Format("{0} Spawners: {1}, Timer: {2}", "Stick", stickSpawners.Count, stickTimer))) {
                //Vector3 randomVector = Random.insideUnitSphere * 5;
                //randomVector.y = 5f;
                //AreaPlacement(5, stickSpawners[Random.Range(0, stickSpawners.Count)].transform.position + randomVector, StickSpawner);
                AddTypeToAreas("Stick");
            } ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 300, 20), string.Format("{0} Spawners: {1}, Timer: {2}", "Sap", sapSpawners.Count, sapTimer))) {
                //Vector3 randomVector = Random.insideUnitSphere * 5;
                //randomVector.y = 5f;
                //AreaPlacement(5, sapSpawners[Random.Range(0, sapSpawners.Count)].transform.position + randomVector, SapSpawner);
                AddTypeToAreas("Sap");
            } ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 300, 20), string.Format("{0} Spawners: {1}, Timer: {2}", "Leaf", leafSpawners.Count, leafTimer))) {
                //Vector3 randomVector = Random.insideUnitSphere * 5;
                //randomVector.y = 5f;
                //AreaPlacement(5, leafSpawners[Random.Range(0, leafSpawners.Count)].transform.position + randomVector, LeafSpawner);
                AddTypeToAreas("Leaf");
            } ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 300, 20), string.Format("{0} Spawners: {1}, Timer: {2}", "Berry", berrySpawners.Count, berryTimer))) {
                //Vector3 randomVector = Random.insideUnitSphere * 5;
                //randomVector.y = 5f;
                //AreaPlacement(5, berrySpawners[Random.Range(0, berrySpawners.Count)].transform.position + randomVector, BerrySpawner);
                AddTypeToAreas("Berry");
            } ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 300, 20), string.Format("{0} Spawners: {1}", "Fish", fishSpawners.Count))) {
                Vector3 randomVector = Random.insideUnitSphere * 5;
                randomVector.y = 5f;
                FishPlacement(5, fishSpawners[Random.Range(0, fishSpawners.Count)].transform.position + randomVector, FishSpawner);
            } ypos += spacing;
        }
    }
    
    IEnumerator GetSpawners() {
        yield return new WaitForFixedUpdate();
        PickupSpawner[] spawners = FindObjectsOfType<PickupSpawner>();
        bananaSpawners.Clear();
        stickSpawners.Clear();
        sapSpawners.Clear();
        leafSpawners.Clear();
        berrySpawners.Clear();
        fishSpawners.Clear();
        foreach (PickupSpawner spawner in spawners) {
            switch(spawner.spawnable.name) {
                case ("Banana"):
                    bananaSpawners.Add(spawner);
                    break;
                case ("Stick"):
                    stickSpawners.Add(spawner);
                    break;
                case ("Sap"):
                    sapSpawners.Add(spawner);
                    break;
                case ("Leaf"):
                    leafSpawners.Add(spawner);
                    break;
                case ("Herb"):
                    berrySpawners.Add(spawner);
                    break;
                case ("Fish"):
                    fishSpawners.Add(spawner);
                    break;
                default:
                    break;
            }
        }
        yield return null;
    }

    public void DetermineSpawnerValues(PlayerStats[] ps, bool areas = false) {
        /*
        Weights:
            SingleRole: 1.5 , 1.0, 1.0
            MutipleRoles: 1.3, 1.2, 1.0 
        
        RoleDefined:
            Sap: Supp, Def, Att
            Banana: Def, Att, Supp
            Leaf: Att
            Stick: Att
            Berry: Supp
            Fish: Def
        */
        int numberOfPlayers = ps.Length;
        if (ps.Length <= 0) return;
        int attackers = 0;
        int defenders = 0;
        int supporters = 0;
        float totalResilience = 0;
        float totalAgility = 0;
        float totalWisdom = 0;
        foreach (PlayerStats player in ps) {
            switch(player.role) {
                case (PlayerStats.Role.Attacker):
                    attackers++;
                    break;
                case (PlayerStats.Role.Defender):
                    defenders++;
                    break;
                case (PlayerStats.Role.Supporter):
                    supporters++;
                    break;
            }
            totalResilience += player.Resilience;
            totalAgility += player.Agility;
            totalWisdom += player.Wisdom;
        }
        players[0] = attackers;
        players[1] = defenders;
        players[2] = supporters;
        //print("Attackers: " + attackers + ", Defenders: " + defenders + ", Supporters: " + supporters);
        StartCoroutine(spawnerMasterLogic(attackers, defenders, supporters, new float[] { 1.5f, 1.3f, 1.2f, 1.0f }, numberOfPlayers, areas));
    }

    IEnumerator spawnerMasterLogic(int attackers, int defenders, int supporters, float[] weights, float playerModifier, bool areas = false) {
        yield return StartCoroutine(GetSpawners());
        // Bananas
        DetermineSpawnTime(bananaSpawners, BananaSpawner,
            (defenders * weights[1] + attackers * weights[2] + supporters * weights[3]), 
            out bananaTimer, areas ? "Banana" : "");
        // Sticks value
        DetermineSpawnTime(stickSpawners, StickSpawner,
            (attackers * weights[0] + supporters * weights[3] + defenders * weights[3]), 
            out stickTimer, areas ? "Stick" : "");
        // Sap value
        DetermineSpawnTime(sapSpawners, SapSpawner,
            (supporters * weights[1] + defenders * weights[2] + attackers * weights[3]), 
            out sapTimer, areas ? "Sap" : "");
        // Leaf value
        DetermineSpawnTime(leafSpawners, LeafSpawner, 
            (attackers * weights[0] + defenders * weights[3] + supporters * weights[3]), 
            out leafTimer, areas ? "Leaf" : "");
        // Berry value
        DetermineSpawnTime(berrySpawners, BerrySpawner,
            (supporters * weights[0] + attackers * weights[3] + defenders * weights[3]), 
            out berryTimer, areas ? "Berry" : "");

        yield return StartCoroutine(GetSpawners());

        foreach (PickupSpawner banana in bananaSpawners) {
            banana.spawnTime = Random.Range(minSpawnTime, bananaTimer);
        } foreach (PickupSpawner stick in stickSpawners) {
            stick.spawnTime = Random.Range(minSpawnTime, stickTimer);
        } foreach (PickupSpawner sap in sapSpawners) {
            sap.spawnTime = Random.Range(minSpawnTime, sapTimer);
        } foreach (PickupSpawner leaf in leafSpawners) {
            leaf.spawnTime = Random.Range(minSpawnTime, leafTimer);
        } foreach (PickupSpawner berry in berrySpawners) {
            berry.spawnTime = Random.Range(minSpawnTime, berryTimer);
        }
        #region Fish value
        float weight = defenders * weights[0] + attackers * weights[3] + supporters * weights[3];
        weight -= fishSpawners.Count;
        while (0 < weight) {
            Vector3 randomVector = Random.insideUnitSphere * 5;
            randomVector.y = 5f;
            FishPlacement(5, fishSpawners[Random.Range(0, fishSpawners.Count)].transform.position + randomVector, FishSpawner);
            weight--;
        }
        foreach (PickupSpawner fish in fishSpawners) {
            fish.spawnTime = defaultSpawnTime / playerModifier;
        }
        #endregion
        yield return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spawners">PickupSpawner list which is to be modified</param>
    /// <param name="spawnerPrefab">Define prefab used if a new spawner is needed</param>
    /// <param name="weight">The impact players choice has put on this type</param>
    /// <param name="timer">The respective timer for the type of spawner</param>
    /// <param name="type">If using CollectableAreas define type of spawner/collectable</param>
    void DetermineSpawnTime(List<PickupSpawner> spawners, GameObject spawnerPrefab, float weight, out float timer, string type = "") {
        weight = weight < 1 ? 1 : weight;
        int spawnerCount;
        spawnerCount = type == "" ? spawners.Count : CountSpawners(type);
        timer = defaultSpawnTime * spawnerCount / weight;
        while (defaultSpawnTime < timer && spawnerCount > 1) { //don't destroy original spawners
            print("RemovingTime " + type + " " + timer + " " + weight);
            if (type == "") Destroy(spawners[spawnerCount - 1].gameObject);
            else if (CountSpawners(type) > 1) RemoveTypeFromAreas(type);
            spawnerCount--;
            timer = defaultSpawnTime * spawnerCount / weight;
        }
        print("InbetweenTime " + type + " " + timer + " " + weight);
        while (timer < minSpawnTime) {
            Vector3 randomVector = Random.insideUnitSphere * 5;
            randomVector.y = 5f;
            if (type == "") AreaPlacement(5, spawners[Random.Range(0, spawners.Count)].transform.position + randomVector, spawnerPrefab);
            else AddTypeToAreas(type, spawnerPrefab);
            spawnerCount++;
            timer = defaultSpawnTime * spawnerCount / weight;
            print("AddingTime " + type + " " + timer + " " + weight);
        }
        timer = defaultSpawnTime * spawnerCount / weight;
    }
    
    GameObject GetPrefab(string type) {
        switch(type) {
            case "Banana":
                return BananaSpawner;
            case "Stick":
                return StickSpawner;
            case "Sap":
                return SapSpawner;
            case "Leaf":
                return LeafSpawner;
            case "Berry":
                return BerrySpawner;
            case "Fish":
                return FishSpawner;
            default:
                return null;
        }
    }

    IEnumerator GetCollectableAreas() {
        collectableAreas = FindObjectsOfType<SpawnerArea>();
        List<string> types = new List<string> { "Banana", "Stick", "Sap", "Leaf", "Berry" };
        foreach (SpawnerArea area in collectableAreas) {
            string type = types[Random.Range(0, types.Count)];
            print("Creating: " + type);
            area.AssignSpawnerType(type);
            yield return new WaitForFixedUpdate();
            area.AddSpawner(type, GetPrefab(type));
            types.Remove(type);
        }
    }

    void AddTypeToAreas(string type) { AddTypeToAreas(type, GetPrefab(type)); }
    void AddTypeToAreas(string type, GameObject prefab) {
        bool wasFull = false;
        foreach (SpawnerArea area in collectableAreas) {
            if (area.SpawnIteration(type) == 1) {
                print("Adding: " + type);
                area.AddSpawner(type, prefab);
                wasFull = false;
                return;
            } else if (area.SpawnIteration(type) == 2) {
                wasFull = true;
                continue;
            }
        }
        if (wasFull) {
            foreach (SpawnerArea area in collectableAreas) {
                if (area.SpawnIteration(type) == 0) {
                    StartCoroutine(AssignAndAdd(area, type));
                    return;
                }
            }
        }
    }

    IEnumerator AssignAndAdd(SpawnerArea area, string type) {
        print("Creating more: " + type);
        area.AssignSpawnerType(type);
        yield return new WaitForFixedUpdate();
        area.AddSpawner(type, GetPrefab(type));
    }

    void RemoveTypeFromAreas(string type) {
        foreach (SpawnerArea area in collectableAreas) {
            if (area.SpawnIteration(type, 3) == 1 || area.SpawnIteration(type, 3) == 2) {
                print("Removing from 3: " + type);
                area.RemoveSpawner(type);
                return;
            }
        }
        foreach (SpawnerArea area in collectableAreas) {
            if (area.SpawnIteration(type, 2) == 1 || area.SpawnIteration(type, 2) == 2) {
                print("Removing from 2: " + type);
                area.RemoveSpawner(type);
                return;
            }
        }
        foreach (SpawnerArea area in collectableAreas) {
            if (area.SpawnIteration(type, 1) == 1 || area.SpawnIteration(type, 1) == 2) {
                print("Removing from 1: " + type);
                area.RemoveSpawner(type);
                return;
            }
        }
    }

    int CountSpawners(string type) {
        int tally = 0;
        foreach (SpawnerArea area in collectableAreas) {
            if (area.SpawnIteration(type) == 1 || area.SpawnIteration(type) == 2) 
                tally += area.CountSpawners(type);
        }
        return tally;
    }

    #region Environment placement
    /// <summary>
    /// Based on a Radius, the method will spawn randomly placed assets within a Spherical area.
    /// Tailored for Fish Spawners
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="where">Where to place the spawner</param>
    /// <param name="asset">What to spawn in water (preferably watery things)</param>
    public void FishPlacement(float radius, Vector3 where, GameObject asset) {
        makeRay:
        Vector3 randomVector = Random.insideUnitSphere * radius * 3;
        randomVector.y = -radius * 3;
        Ray ray = new Ray(where, randomVector);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, radius * 2, (1 << 9) | (1 << 4))) {
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Water")) {

                CreateObject(asset, hitInfo.point - Vector3.up * 1.5f, Quaternion.FromToRotation(Vector3.up, hitInfo.normal), gameObject);

                Debug.DrawLine(where, hitInfo.point, Color.red, 100f);
            } else {
                goto makeRay;
            }
        }
    }

    /// <summary>
    /// Based on a Radius, the method will spawn randomly placed assets within a Spherical area.
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="where">Where to place the spawner</param>
    /// <param name="asset"></param>
    public void AreaPlacement(float radius, Vector3 where, GameObject asset) {
        int groundLayerMask = (1 << 9);//The 'Ground' Layers we want to check.
        int amountWeNeed = 0;
        #region UnityTerrain: If we are using a Unity Terrain.
        if (isUsingUnityTerrain) {
            TerrainData myTerrainData = terrainParentInfo.GetTerrainData();

            makeRay:
            Vector3 randomVector = Random.insideUnitSphere * radius * 3;
            randomVector.y = -radius * 3;
            Ray ray = new Ray(where, randomVector);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, radius * 2, groundLayerMask | (1 << 0) | (1 << 10) | (1 << 11))) {
                if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Default") || 
                    hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Environment") ||
                    hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Trees")) {
                    goto makeRay;
                } else {
                    Vector3 interpolatedNormal = myTerrainData.GetInterpolatedNormal(hitInfo.point.x, hitInfo.point.y);

                    CreateObject(asset, hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal), gameObject);

                    Debug.DrawLine(where, hitInfo.point, Color.red, 100f);
                    amountWeNeed++;
                }
            }
        }
        #endregion
    }
    
    public void CreateObject(GameObject go, Vector3 p, Quaternion r, GameObject g) {
        var spawner = (GameObject)GameObject.Instantiate(go, p, r);
        spawner.transform.SetParent(g.transform);
        NetworkServer.Spawn(spawner);
    }
    #endregion
}
