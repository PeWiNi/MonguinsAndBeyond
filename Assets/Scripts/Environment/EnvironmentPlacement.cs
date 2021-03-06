﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

public class EnvironmentPlacement : NetworkBehaviour
{
    public enum Placement // your custom enumeration
    {
        Random,
        Area,
        Fill,
        None
    };
    public Placement placementState = Placement.None;//Default None.
    public List<GameObject> assets;
    [Tooltip("Required for PlacementState [Area, Fill]")]
    [Range(0, 1000)]
    public int maxNumberOfAssets = 500;
    [Tooltip("Required for PlacementState [Area]\nThis value will be the radius around the 'startingPoint' to spagameObjectn assets")]
    public int areaRadius = 25;
    [Tooltip("Required for PlacementState [Area, Random]")]
    public int assetID;
    [Tooltip("Required for PlacementState [Fill]")]
    [Range(0, 100)]
    public float heightMin = 25f;
    [Tooltip("Required for PlacementState [Fill]")]
    [Range(0, 100)]
    public float heightMax = 75;
    [Tooltip("Required for PlacementState [Fill]")]
    public bool isBetween;
    [Tooltip("Required for PlacementState [Area]\nDefault (True)")]
    public bool isUsingUnityTerrain = true;

    int randomSeedValue = 42;

    public GameObject terrainParent = null;
    TerrainInfo terrainParentInfo;

    void Start() {
        if (!isServer)
            return;
        if (terrainParent != null)
            terrainParentInfo = terrainParent.GetComponent<TerrainInfo>();
        //Random.seed = randomSeedValue;//Sets the seed value of the Random class.
        if (placementState == Placement.Random)
            RandomPlacement(this.maxNumberOfAssets, this.assets);
        if (placementState == Placement.Area)
            AreaPlacement(this.areaRadius, this.maxNumberOfAssets, this.assetID, true);
        if (placementState == Placement.Fill)
            FillPlacement(this.heightMin, this.heightMax, this.isBetween, this.maxNumberOfAssets);
    }

    /// <summary>
    /// Fill environment with assets between 2 height values.
    /// Based on the values and the boolean expression the method will either add assets in-between the ranges or outside the ranges.
    /// Takes random assets in the 'Assets' GameObject List.
    /// </summary>
    /// <param name="heightMin"></param>
    /// <param name="heightMax"></param>
    /// <param name="isBetween"></param>
    /// <param name="maxNumberOfAssets"></param>
    public void FillPlacement(float heightMin, float heightMax, bool isBetween, int maxNumberOfAssets) {
        Mesh mesh = null;
        mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] allVertices = mesh.vertices;
        int amountWeNeed = 0;
        List<Vector3> verticesWithinBoundaries = new List<Vector3>();
        for (int x = 0; x < allVertices.Length; x++) {
            Vector3 vertex = allVertices[x];
            vertex.x *= transform.localScale.x;
            vertex.y *= transform.localScale.y;
            vertex.z *= transform.localScale.z;
            if (isBetween && (vertex.y > heightMin && vertex.y < heightMax)) {
                Debug.DrawLine(vertex, vertex + Vector3.up, Color.blue, 100f);
                verticesWithinBoundaries.Add(vertex);
            }
            else if (!isBetween && (vertex.y < heightMin || vertex.y > heightMax)) {
                Debug.DrawLine(vertex, vertex + Vector3.up, Color.blue, 100f);
                verticesWithinBoundaries.Add(vertex);
            }
            else {
                Debug.DrawLine(vertex, vertex + Vector3.up, Color.red, 100f);
            }
        }
        while (amountWeNeed < maxNumberOfAssets) {
            //Get a random object from the environmentAssetsPool and assign it to the 'go' GameObject.
            GameObject assetGameObject = assets[Random.Range(0, assets.Count)];
            //Access a random vertex from the vertices of the mesh.
            Vector3 randomVertex = verticesWithinBoundaries[Random.Range(0, verticesWithinBoundaries.Count)];
            //Get a point from the Random.onUnitSphere and apply a distance from the origin of that point (this is where the Ray later will have its origin).
            Vector3 origin = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
            if (isBetween) {
                while (origin.y <= heightMin || origin.y >= heightMax) {
                    origin = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
                }
            }
            if (!isBetween) {
                while (origin.y >= heightMin && origin.y <= heightMax) {
                    origin = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
                }
            }
            RaycastHit hit;
            if (Physics.Raycast(origin, randomVertex, out hit)) {
                if (isBetween && (hit.point.y > heightMin && hit.point.y < heightMax)) {
                    GameObject go = Instantiate(assetGameObject, hit.point, Quaternion.identity) as GameObject;//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                    //if (newSection != null)
                    //    go.transform.parent = newSection.transform;
                    //else
                    go.transform.parent = transform;
                    amountWeNeed++;
                }
                if (!isBetween && (hit.point.y < heightMin || hit.point.y > heightMax)) {
                    GameObject go = Instantiate(assetGameObject, hit.point, Quaternion.identity) as GameObject;//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                    //if (newSection != null)
                    //    go.transform.parent = newSection.transform;
                    //else
                    go.transform.parent = transform;
                    amountWeNeed++;
                }
            }
        }
    }

    /// <summary>
    /// Based on a Radius, the method will spawn randomly placed assets within a Spherical area.
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="maxNumberOfAssets"></param>
    /// <param name="assetID"></param>
    /// <param name="randomAssets"></param>
    public void AreaPlacement(float radius, int maxNumberOfAssets, int assetID, bool randomAssets, Vector3 pos = new Vector3()) {
        int groundLayerMask = (1 << 9);//The 'Ground' Layers we want to check.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, groundLayerMask);//Get a collection of all colliders touched or within the sphere.
        GameObject assetGameObject = assets[assetID];//Get the environment asset you want to fill this area with.
        int amountWeNeed = 0;
        bool hasEnsuredCollectable = false;// Used to ensure that we do atleast have '1' Collectable created.
        while (amountWeNeed < maxNumberOfAssets) {
            #region Meshes: If we are using Normal 3D Meshes
            //int randomHitCollider = 0;
            //Mesh mesh = null;
            //Vector3[] vertices = null;
            //hitColliders = Physics.OverlapSphere(gameObject.transform.position, radius, groundLayerMask);
            //List<Vector3> verticesWithinRadius = new List<Vector3>();
            //List<Vector3> normalsWithinRadius = new List<Vector3>();
            //for (int i = 0; i < vertices.Length; i++) {
            //    Vector3 vertex = vertices[i];
            //    vertex.x *= hitColliders[randomHitCollider].transform.localScale.x;
            //    vertex.y *= hitColliders[randomHitCollider].transform.localScale.y;
            //    vertex.z *= hitColliders[randomHitCollider].transform.localScale.z;
            //    if (Vector3.Distance(transform.position, vertex) < radius) {
            //        verticesWithinRadius.Add(vertex);
            //        normalsWithinRadius.Add(mesh.normals[i]);
            //    }
            //}
            //int randomVerticesWithinRadius = Random.Range(0, verticesWithinRadius.Count);
            //Vector3 randomVertex = verticesWithinRadius[randomVerticesWithinRadius];
            //Vector3 randomNormal = normalsWithinRadius[randomVerticesWithinRadius];

            //GameObject go = Instantiate(assetGameObject, randomVertex, Quaternion.FromToRotation(Vector3.up, randomNormal)) as GameObject;//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
            //go.transform.parent = transform;
            //amountWeNeed++;
            #endregion

            #region UnityTerrain: If we are using a Unity Terrain.
            if (isUsingUnityTerrain) {
                List<Vector3> normalsWithinRadius = new List<Vector3>();
                TerrainData myTerrainData = terrainParentInfo.GetTerrainData();
                if (randomAssets && hasEnsuredCollectable) {
                    assetGameObject = assets[Random.Range(0, assets.Count)];
                }
                else {
                    hasEnsuredCollectable = true;
                    assetGameObject = assets[0];
                }
                //TerrainData myTerrainData = hitColliders[0].gameObject.GetComponent<TerrainCollider>().terrainData;
                //float[,] allHeights = myTerrainData.GetHeights(0, 0, myTerrainData.heightmapResolution, myTerrainData.heightmapResolution);
                //print("AllHeights length = " + allHeights.Length);
                //for (int x = 0; x < allHeights.GetLength(0); x++) {
                //    for (int y = 0; y < allHeights.GetLength(1); y++) {
                //        //print("Height = " + allHeights[x, y] + ", X = " + x + ", Y = " + y);
                //    }
                //}

                //Vector3 first = transform.position;
                //Vector3 second = new Vector3(first.x * Random.Range(-radius, radius), -first.y * radius, first.z * Random.Range(-radius, radius));
                //Ray ray = new Ray(first, second);
                Vector3 randomVector = Random.insideUnitSphere * 500;
                randomVector.y = -500f;
                Vector3 position = pos == new Vector3() ? transform.position : pos;
                Ray ray = new Ray(position, randomVector);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, radius * 2, groundLayerMask)) {
                    if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Environment") ||
                        hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Trees") || 
                        hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast")) {
                        continue;
                    }
                    else {
                        Vector3 interpolatedNormal = myTerrainData.GetInterpolatedNormal(hitInfo.point.x, hitInfo.point.y);
                        Vector3 something = (interpolatedNormal - hitInfo.point) / myTerrainData.size.magnitude;
                        print("something = " + something);
                        //GameObject go = Instantiate(assetGameObject, hitInfo.point, Quaternion.identity) as GameObject;
                        GameObject go = Instantiate(assetGameObject, hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal)) as GameObject;
                        go.transform.parent = transform;
                        Debug.DrawLine(position, hitInfo.point, Color.red, 100f);
                        amountWeNeed++;
                        NetworkServer.Spawn(go);
                    }
                }
            }
            #endregion
        }
    }

    /// <summary>
    /// Randomly placing different assets placed in the assets list.
    /// </summary>
    /// <param name="newSection"></param>
    /// <param name="newVertices"></param>
    /// <param name="maxNumberOfAssets"></param>
    /// <param name="assets"></param>
    //public void RandomPlacement(GameObject newSection, Vector3[] newVertices, int maxNumberOfAssets, List<GameObject> assets)
    public void RandomPlacement(int maxNumberOfAssets, List<GameObject> assets) {
        float terrainSize = GetComponent<Terrain>().terrainData.size.magnitude;
        Collider coll = GetComponent<TerrainCollider>();
        Vector3 origin = transform.TransformPoint(transform.position);
        //origin.x += terrainSize;
        //origin.z += terrainSize;
        int amountWeNeed = 0;
        while (amountWeNeed < maxNumberOfAssets) {
            Vector3 end = new Vector3(Random.Range(0f, terrainSize), (float)GetComponent<Terrain>().terrainData.heightmapHeight, Random.Range(0f, terrainSize));
            Debug.DrawRay(origin, end, Color.red, 100f);
            RaycastHit hit;
            Ray ray = new Ray(origin, end);
            amountWeNeed++;
            if (coll.Raycast(ray, out hit, 1000f)) {
                if (hit.transform.gameObject.layer != (1 << 9))
                    continue;
                GameObject go = Instantiate(assets[Random.Range(0, assets.Count)], hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                go.transform.parent = hit.transform;
                //amountWeNeed++;
            }
        }
    }

    void OnDrawGizmosSelected() {
        //Vector3 center = gameObject.GetComponent<MeshRenderer>().bounds.center;
        //float radius = gameObject.GetComponent<MeshRenderer>().bounds.extents.magnitude;
        Vector3 center = gameObject.transform.position;
        float radius = this.areaRadius;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(center, radius);
    }
}
