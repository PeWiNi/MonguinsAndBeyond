using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class MasterPickupSpawner : NetworkBehaviour {
	private NetworkManager manager;

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

    int bananaValue;
    int stickValue;
    int sapValue;
    int leafValue;
    int berryValue;

    int[] players = new int[3];

    public float defaultSpawnValue = 1;
    public float maxSpawnValue = 5;
    public float defaultSpawnTime = 30;

    // Use this for initialization
    void Start () {
        if (terrainParent != null)
            terrainParentInfo = terrainParent.GetComponent<TerrainInfo>();
        manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        //GetSpawners();
        //AreaPlacement(areaRadius, transform.position, BerrySpawner);
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
            GetSpawners();
            DetermineSpawnerValues(FindObjectsOfType<PlayerStats>());
        }
    }

    void OnGUI() {
        if (!isServer || !showGUI) return;

        int xpos = 10 + offsetX;
        int ypos = 10 + offsetY;
        int spacing = 24;

        if (NetworkServer.active) {
            GUI.Label(new Rect(xpos, ypos, 300, 20), "Server: address=" + manager.networkAddress + " port=" + manager.networkPort);
            ypos += spacing;

            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Rebalance Spawners")) {
                GetSpawners();
                DetermineSpawnerValues(FindObjectsOfType<PlayerStats>());
            }
            int playersSum = players[0] + players[1] + players[2];
            ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 500, 20), string.Format("Total # of players: {0}. Defenders: {1}, Attackers: {2}, Supporters: {3}", playersSum, players[1], players[0], players[2]))) { print("It's not very effective!"); }
            ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 300, 20), string.Format("{0} Spawners: {1}, Timer: {2}, Value: {3}", "Banana", bananaSpawners.Count, bananaTimer, bananaValue))) {
                Vector3 randomVector = Random.insideUnitSphere * 5;
                randomVector.y = 5f;
                AreaPlacement(5, bananaSpawners[Random.Range(0, bananaSpawners.Count)].transform.position + randomVector, BananaSpawner);
            } ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 300, 20), string.Format("{0} Spawners: {1}, Timer: {2}, Value: {3}", "Stick", stickSpawners.Count, stickTimer, stickValue))) {
                Vector3 randomVector = Random.insideUnitSphere * 5;
                randomVector.y = 5f;
                AreaPlacement(5, stickSpawners[Random.Range(0, stickSpawners.Count)].transform.position + randomVector, StickSpawner);
            } ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 300, 20), string.Format("{0} Spawners: {1}, Timer: {2}, Value: {3}", "Sap", sapSpawners.Count, sapTimer, sapValue))) {
                Vector3 randomVector = Random.insideUnitSphere * 5;
                randomVector.y = 5f;
                AreaPlacement(5, sapSpawners[Random.Range(0, sapSpawners.Count)].transform.position + randomVector, SapSpawner);
            } ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 300, 20), string.Format("{0} Spawners: {1}, Timer: {2}, Value: {3}", "Leaf", leafSpawners.Count, leafTimer, leafValue))) {
                Vector3 randomVector = Random.insideUnitSphere * 5;
                randomVector.y = 5f;
                AreaPlacement(5, leafSpawners[Random.Range(0, leafSpawners.Count)].transform.position + randomVector, LeafSpawner);
            } ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 300, 20), string.Format("{0} Spawners: {1}, Timer: {2}, Value: {3}", "Berry", berrySpawners.Count, berryTimer, berryValue))) {
                Vector3 randomVector = Random.insideUnitSphere * 5;
                randomVector.y = 5f;
                AreaPlacement(5, berrySpawners[Random.Range(0, berrySpawners.Count)].transform.position + randomVector, BerrySpawner);
            } ypos += spacing;
            if (GUI.Button(new Rect(xpos, ypos, 300, 20), string.Format("{0} Spawners: {1}", "Fish", fishSpawners.Count))) {
                Vector3 randomVector = Random.insideUnitSphere * 5;
                randomVector.y = 5f;
                FishPlacement(5, fishSpawners[Random.Range(0, fishSpawners.Count)].transform.position + randomVector, FishSpawner);
            } ypos += spacing;
        }

    }

    void GetSpawners() {
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
    }

    public void DetermineSpawnerValues(PlayerStats[] ps) {
        /*
        Weights:
            SingleRole: 1.5 
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
        print("Attackers: " + attackers + ", Defenders: " + defenders + ", Supporters: " + supporters);
        spawnerValueSetter(attackers, defenders, supporters, new float[] { 1.5f, 1.3f, 1.2f, 1.0f }, numberOfPlayers);
    }

    void spawnerValueSetter(int attackers, int defenders, int supporters, float[] weights, float playerModifier) {
        int spawnerCount;
        float weight;
        // Bananas value
        weight = defenders * weights[1] + attackers * weights[2] + supporters * weights[3];
        weight /= bananaSpawners.Count / playerModifier;
        bananaValue = Mathf.CeilToInt(weight > defaultSpawnValue ? weight : defaultSpawnValue);
        foreach (PickupSpawner banana in bananaSpawners) {
            //banana.value = bananaValue;
            banana.spawnTime = defaultSpawnTime / playerModifier;
        }
        // Sticks value
        weight = attackers * weights[0];
        weight /= stickSpawners.Count / playerModifier;
        stickValue = Mathf.CeilToInt(weight > defaultSpawnValue ? weight : defaultSpawnValue);
        foreach (PickupSpawner stick in stickSpawners) {
            //stick.value = stickValue;
            stick.spawnTime = defaultSpawnTime / playerModifier;
        }
        // Sap value
        weight = supporters * weights[1] + defenders * weights[2] + attackers * weights[3];
        weight /= sapSpawners.Count / playerModifier;
        sapValue = Mathf.CeilToInt(weight > defaultSpawnValue ? weight : defaultSpawnValue);
        foreach (PickupSpawner sap in sapSpawners) {
            //sap.value = sapValue;
            sap.spawnTime = defaultSpawnTime / playerModifier;
        }
        // Leaf value
        weight = attackers * weights[0];
        weight /= playerModifier;
        spawnerCount = leafSpawners.Count;
        leafTimer = defaultSpawnTime / weight * spawnerCount;
        while (60 < leafTimer) { //TODO: don't destroy original spawners
            Destroy(leafSpawners[spawnerCount - 1].gameObject);
            spawnerCount--;
            leafTimer = defaultSpawnTime / playerModifier * spawnerCount;
        }
        while(leafTimer < 15) {
            Vector3 randomVector = Random.insideUnitSphere * 5;
            randomVector.y = 5f;
            AreaPlacement(5, leafSpawners[Random.Range(0, spawnerCount)].transform.position + randomVector, LeafSpawner);
            spawnerCount++;
            leafTimer = defaultSpawnTime / weight * spawnerCount;
        }
        leafTimer = defaultSpawnTime / weight * spawnerCount;
        leafValue = Mathf.CeilToInt(weight > defaultSpawnValue ? weight : defaultSpawnValue);
        GetSpawners();
        foreach (PickupSpawner leaf in leafSpawners) {
            //leaf.value = leafValue;
            leaf.spawnTime = leafTimer;
        }
        // Berry value
        weight = supporters * weights[0];
        weight /= berrySpawners.Count / playerModifier;
        spawnerCount = berrySpawners.Count;
        while (maxSpawnValue < weight) {
            Vector3 randomVector = Random.insideUnitSphere * 5;
            randomVector.y = 5f;
            AreaPlacement(5, berrySpawners[Random.Range(0, spawnerCount)].transform.position + randomVector, BerrySpawner);
            spawnerCount++;
            weight = supporters * weights[0];
            weight /= spawnerCount / playerModifier;
        }
        berryTimer = defaultSpawnTime / playerModifier * spawnerCount;
        berryValue = Mathf.CeilToInt(weight > defaultSpawnValue ? weight : defaultSpawnValue);
        foreach (PickupSpawner berry in berrySpawners) {
            //berry.value = berryValue;
            berry.spawnTime = defaultSpawnTime / playerModifier;
        }
        #region Fish value
        weight = defenders * weights[0];
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

    }

    /// <summary>
    /// Based on a Radius, the method will spawn randomly placed assets within a Spherical area.
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="where">Where to place the spawner</param>
    /// <param name="asset"></param>
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
}
