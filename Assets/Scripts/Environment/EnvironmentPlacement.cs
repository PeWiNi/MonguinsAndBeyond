using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnvironmentPlacement : MonoBehaviour
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
    [Range(0, 5000)]
    public int maxNumberOfAssets = 2500;
    [Tooltip("The GameObject which is the center point for spawning.\nRelated to PlacementState [Area]")]
    public GameObject startingPoint;
    [Tooltip("The radius around the 'startingPoint' to spawn assets")]
    [Range(0, 50)]
    public int radius = 25;
    [Tooltip("Required for PlacementState [Area, Random]")]
    public int assetID;

    [Tooltip("Required for PlacementState [Fill]")]
    [Range(0, 100)]
    public float betweenMin = 25f;
    [Tooltip("Required for PlacementState [Fill]")]
    [Range(0, 100)]
    public float betweenMax = 75;
    [Tooltip("Required for PlacementState [Fill]")]
    public bool isBetween;

    private MeshFilter[] meshFilters;
    private Mesh mesh;

    void Start()
    {
        meshFilters = GetComponentsInChildren<MeshFilter>();
        mesh = GetComponent<MeshFilter>().mesh;
        if (placementState == Placement.Random)
            RandomPlacement(this.assetID);
        if (placementState == Placement.Area)
            AreaPlacement(this.startingPoint, this.radius, this.maxNumberOfAssets, this.assetID);
        if (placementState == Placement.Fill)
            FillPlacement(betweenMin, betweenMax, isBetween);
    }

    /// <summary>
    /// Fill environment with assets between 2 height values.
    /// </summary>
    void FillPlacement(float betweenMin, float betweenMax, bool isBetween)
    {
        Vector3[] vertices = mesh.vertices;
        int amountWeNeed = 0;
        while (amountWeNeed < maxNumberOfAssets)
        {
            //Get a random object from the environmentAssetsPool and assign it to the 'go' GameObject.
            GameObject go = assets[(int)Random.Range(0f, assets.Count)];
            //Access a random vertex from the vertices of the mesh.
            Vector3 randomVertex = vertices[Random.Range(0, vertices.Length)];
            //Get a point from the Random.onUnitSphere and apply a distance from the origin of that point (this is where the Ray later will have its origin).
            Vector3 origin = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
            if (isBetween)
            {
                while (origin.y < betweenMin || origin.y > betweenMax)
                {
                    origin = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
                }
            }
            if (!isBetween)
            {
                while (origin.y > betweenMin && origin.y < betweenMax)
                {
                    origin = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
                }
            }
            RaycastHit hit;
            if (Physics.Raycast(origin, randomVertex, out hit))
            {
                if (isBetween && hit.point.y > betweenMin && hit.point.y < betweenMax)
                {
                    Debug.DrawRay(origin, randomVertex, Color.yellow, 150f);
                    Instantiate(go, hit.point, Quaternion.identity);//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                    amountWeNeed++;
                }
                if (!isBetween && hit.point.y < betweenMin || hit.point.y > betweenMax)
                {
                    Debug.DrawRay(origin, randomVertex, Color.yellow, 150f);
                    Instantiate(go, hit.point, Quaternion.identity);//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                    amountWeNeed++;
                }
            }
        }
    }

    /// <summary>
    /// Based on the following; 
    /// 1) GameObject Starting Point (this will be the main focus and spawning point for the assets). 
    /// 2) Radius (the boundary around the Starting Point, using a OverlapSphere). 
    /// 3) Amount Of Assets to spawn within this Radius boundary.
    /// 4) The Asset ID to spawn at random locations within the Radius boundary.
    /// The map will be filled with environmental assets at the geiven Location based on values.
    /// </summary>
    /// <param name="environmentAsset"></param>
    void AreaPlacement(GameObject startingPoint, float radius, int amountOfAssets, int assetID)
    {
        int groundLayerMask = (1 << 9);//The 'Ground' Layers we want to check.
        //Get a collection of all colliders touched or within the sphere.
        Collider[] hitColliders = Physics.OverlapSphere(startingPoint.transform.position, radius, groundLayerMask);
        //Get the environment asset you want to fill this area with.
        GameObject go = assets[assetID];
        int amountWeNeed = 0;
        while (amountWeNeed < amountOfAssets)
        {
            int randomHitCollider = Random.Range(0, hitColliders.Length);
            Mesh mesh = hitColliders[randomHitCollider].transform.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] vertices = hitColliders[randomHitCollider].transform.GetComponent<MeshFilter>().sharedMesh.vertices;
            List<Vector3> verticesWithinRadius = new List<Vector3>();
            List<Vector3> normalsWithinRadius = new List<Vector3>();
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                vertex.x *= hitColliders[randomHitCollider].transform.localScale.x;
                vertex.y *= hitColliders[randomHitCollider].transform.localScale.y;
                vertex.z *= hitColliders[randomHitCollider].transform.localScale.z;
                if (Vector3.Distance(startingPoint.transform.position, vertex) < radius)
                {
                    verticesWithinRadius.Add(vertex);
                    normalsWithinRadius.Add(mesh.normals[i]);
                }
            }
            int randomVerticesWithinRadius = Random.Range(0, verticesWithinRadius.Count - 1);
            Vector3 randomVertex = verticesWithinRadius[randomVerticesWithinRadius];
            Vector3 randomNormal = normalsWithinRadius[randomVerticesWithinRadius];
            Instantiate(go, randomVertex, Quaternion.FromToRotation(Vector3.up, randomNormal));//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
            amountWeNeed++;
        }
    }

    /// <summary>
    /// Create randomly placed environment asset.
    /// Based on two vertices, a raycast will be perfomred between those two and spawn the asset on hit point.
    /// </summary>
    void RandomPlacement(int assetID)
    {
        for (int i = 0; i < meshFilters.Length; i++)
        {
            //Vector3[] vertices = mesh.vertices;
            Vector3[] vertices = meshFilters[i].mesh.vertices;
            //if (meshFilters[i].name == "mapHandler")
            //    continue;
            //print("MeshFilter name = " + meshFilters[i].name);
            //Mesh mesh = meshFilters[i].mesh;
            //Vector3[] vertices = mesh.vertices;
            //Vector3[] normals = mesh.normals;
            //int x = 0;
            //while (x < vertices.Length)
            //{
            //    vertices[x] += normals[x] * Mathf.Sin(Time.time);
            //    x++;
            //}
            //mesh.vertices = vertices;
            for (int j = 0; j < vertices.Length; j++)
            {
                Vector3 randomVertex = mesh.vertices[Random.Range(0, vertices.Length)];
                Vector3 vertex = mesh.vertices[j];
                randomVertex.x *= transform.lossyScale.x;
                randomVertex.y *= transform.lossyScale.y;
                randomVertex.z *= transform.lossyScale.z;
                vertex.x *= transform.lossyScale.x;
                vertex.y *= transform.lossyScale.y;
                vertex.z *= transform.lossyScale.z;
                //Debug.DrawRay(mesh.vertices[j], Vector3.up, Color.cyan, 50f);
                RaycastHit hit;
                if (Physics.Raycast(randomVertex, vertex, out hit))
                {
                    Debug.DrawLine(randomVertex, vertex, Color.cyan, 100f);//CAUTION: Giant web of DOOM appears beware!
                    Instantiate(assets[assetID], hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                }
            }
        }
    }
}
