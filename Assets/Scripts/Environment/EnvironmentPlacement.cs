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
    [Tooltip("Required for PlacementState [Area, Fill]")]
    [Range(0, 5000)]
    public int maxNumberOfAssets = 2500;
    [Tooltip("Required for PlacementState [Area]\nThis value will be the radius around the 'startingPoint' to spagameObjectn assets")]
    [Range(0, 50)]
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

    private MeshFilter[] meshFilters;
    private Dictionary<GameObject, Vector3[]> sections = new Dictionary<GameObject, Vector3[]>();
    int randomSeedValue = 42;

    void Start()
    {
        Random.seed = randomSeedValue;//Sets the seed value of the Random class.
        if (placementState == Placement.Random)
            RandomPlacement(null, null, maxNumberOfAssets, assets);
        if (placementState == Placement.Area)
            AreaPlacement(gameObject, this.areaRadius, this.maxNumberOfAssets, this.assetID);
        if (placementState == Placement.Fill)
            FillPlacement(null, null, this.heightMin, this.heightMax, this.isBetween, this.maxNumberOfAssets);
    }

    public void AddSection(GameObject newSection, Vector3[] newVertices)
    {
        sections.Add(newSection, newVertices);
        //RandomPlacement(newSection, newVertices, this.maxNumberOfAssets, this.assets);
        AreaPlacement(newSection, 100f, maxNumberOfAssets, 0);
        //FillPlacement(newSection, newVertices, heightMin, heightMax, isBetween, maxNumberOfAssets);
    }

    /// <summary>
    /// Fill environment with assets between 2 height values.
    /// Based on the values and the boolean expression the method will either add assets in-between the ranges or outside the ranges.
    /// Takes random assets in the 'Assets' GameObject List.
    /// </summary>
    public void FillPlacement(GameObject newSection, Vector3[] newVertices, float heightMin, float heightMax, bool isBetween, int maxNumberOfAssets)
    {
        Mesh mesh = null;
        if (newSection != null)
            mesh = newSection.GetComponent<MeshFilter>().sharedMesh;
        else
            mesh = this.gameObject.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] allVertices = mesh.vertices;
        if (newVertices != null)
            allVertices = newVertices;
        int amountWeNeed = 0;
        List<Vector3> verticesWithinBoundaries = new List<Vector3>();
        for (int x = 0; x < allVertices.Length; x++)
        {
            Vector3 vertex = allVertices[x];
            vertex.x *= transform.localScale.x;
            vertex.y *= transform.localScale.y;
            vertex.z *= transform.localScale.z;
            if (isBetween && (vertex.y > heightMin && vertex.y < heightMax))
            {
                Debug.DrawLine(vertex, vertex + Vector3.up, Color.blue, 100f);
                verticesWithinBoundaries.Add(vertex);
            }
            else if (!isBetween && (vertex.y < heightMin || vertex.y > heightMax))
            {
                Debug.DrawLine(vertex, vertex + Vector3.up, Color.blue, 100f);
                verticesWithinBoundaries.Add(vertex);
            }
            else
            {
                Debug.DrawLine(vertex, vertex + Vector3.up, Color.red, 100f);
            }
        }
        while (amountWeNeed < maxNumberOfAssets)
        {
            //Get a random object from the environmentAssetsPool and assign it to the 'go' GameObject.
            GameObject assetGameObject = assets[Random.Range(0, assets.Count)];
            //Access a random vertex from the vertices of the mesh.
            Vector3 randomVertex = verticesWithinBoundaries[Random.Range(0, verticesWithinBoundaries.Count)];
            //Get a point from the Random.onUnitSphere and apply a distance from the origin of that point (this is where the Ray later will have its origin).
            Vector3 origin = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
            if (isBetween)
            {
                while (origin.y <= heightMin || origin.y >= heightMax)
                {
                    origin = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
                }
            }
            if (!isBetween)
            {
                while (origin.y >= heightMin && origin.y <= heightMax)
                {
                    origin = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
                }
            }
            RaycastHit hit;
            if (Physics.Raycast(origin, randomVertex, out hit))
            {
                if (isBetween && (hit.point.y > heightMin && hit.point.y < heightMax))
                {
                    GameObject go = Instantiate(assetGameObject, hit.point, Quaternion.identity) as GameObject;//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                    if (newSection != null)
                        go.transform.parent = newSection.transform;
                    else
                        go.transform.parent = transform;
                    amountWeNeed++;
                }
                if (!isBetween && (hit.point.y < heightMin || hit.point.y > heightMax))
                {
                    GameObject go = Instantiate(assetGameObject, hit.point, Quaternion.identity) as GameObject;//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                    if (newSection != null)
                        go.transform.parent = newSection.transform;
                    else
                        go.transform.parent = transform;
                    amountWeNeed++;
                }
            }
        }
    }

    /// <summary>
    /// Based on a Radius, the method will spawn randomly placed assets within a circular area.
    /// </summary>
    /// <param name="newSection"></param>
    /// <param name="radius"></param>
    /// <param name="maxNumberOfAssets"></param>
    /// <param name="assetID"></param>
    public void AreaPlacement(GameObject newSection, float radius, int maxNumberOfAssets, int assetID)
    {
        int groundLayerMask = (1 << 9);//The 'Ground' Layers we want to check.
        //Get a collection of all colliders touched or within the sphere.
        Collider[] hitColliders = Physics.OverlapSphere(newSection.transform.position, radius, groundLayerMask);
        //Get the environment asset you want to fill this area with.
        GameObject assetGameObject = assets[assetID];
        int amountWeNeed = 0;
        while (amountWeNeed < maxNumberOfAssets)
        {
            int randomHitCollider = 0;
            Mesh mesh = null;
            Vector3[] vertices = null;
            if (newSection == null && hitColliders.Length == 0)
            {
                hitColliders = Physics.OverlapSphere(gameObject.transform.position, radius, groundLayerMask);
            }
            else
            {
                randomHitCollider = Random.Range(0, hitColliders.Length);
                mesh = hitColliders[randomHitCollider].transform.GetComponent<MeshFilter>().sharedMesh;
                vertices = hitColliders[randomHitCollider].transform.GetComponent<MeshFilter>().sharedMesh.vertices;

                Debug.DrawRay(mesh.bounds.max, mesh.bounds.max + Vector3.up * 10, Color.red, 100f);
            }
            List<Vector3> verticesWithinRadius = new List<Vector3>();
            List<Vector3> normalsWithinRadius = new List<Vector3>();
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                vertex.x *= hitColliders[randomHitCollider].transform.localScale.x;
                vertex.y *= hitColliders[randomHitCollider].transform.localScale.y;
                vertex.z *= hitColliders[randomHitCollider].transform.localScale.z;
                if (Vector3.Distance(newSection.transform.position, vertex) < radius)
                {
                    verticesWithinRadius.Add(vertex);
                    normalsWithinRadius.Add(mesh.normals[i]);
                }
            }
            int randomVerticesWithinRadius = Random.Range(0, verticesWithinRadius.Count);
            Vector3 randomVertex = verticesWithinRadius[randomVerticesWithinRadius];
            Vector3 randomNormal = normalsWithinRadius[randomVerticesWithinRadius];
            GameObject go = Instantiate(assetGameObject, randomVertex, Quaternion.FromToRotation(Vector3.up, randomNormal)) as GameObject;//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
            go.transform.parent = newSection.transform;
            amountWeNeed++;
        }
    }

    /// <summary>
    /// Randomly placing different assets placed in the assets list.
    /// </summary>
    /// <param name="newSection"></param>
    /// <param name="newVertices"></param>
    /// <param name="maxNumberOfAssets"></param>
    /// <param name="assets"></param>
    public void RandomPlacement(GameObject newSection, Vector3[] newVertices, int maxNumberOfAssets, List<GameObject> assets)
    {
        if (newSection != null && newVertices.Length > 0)
        {
            Mesh mesh = newSection.GetComponent<MeshFilter>().sharedMesh;
            int amountWeNeed = 0;
            while (amountWeNeed < maxNumberOfAssets)
            {
                Vector3 origin = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max);
                Vector3 end = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max);
                RaycastHit hit;
                if (Physics.Raycast(origin, end, out hit))
                {
                    //Debug.DrawLine(origin, end, Color.blue, 100f);//CAUTION: Giant web of DOOM appears beware!
                    GameObject go = Instantiate(assets[Random.Range(0, assets.Count)], hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                    go.transform.parent = hit.transform;
                    amountWeNeed++;
                }
            }
        }
        else
        {
            meshFilters = GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                Vector3[] vertices = meshFilters[i].mesh.vertices;
                for (int j = 0; j < vertices.Length; j++)
                {
                    Vector3 randomVertex = vertices[Random.Range(0, vertices.Length)];
                    Vector3 vertex = vertices[j];
                    randomVertex.x *= transform.lossyScale.x;
                    randomVertex.y *= transform.lossyScale.y;
                    randomVertex.z *= transform.lossyScale.z;
                    vertex.x *= transform.lossyScale.x;
                    vertex.y *= transform.lossyScale.y;
                    vertex.z *= transform.lossyScale.z;
                    //Debug.DrawRay(vertices[j], Vector3.up * 5, Color.cyan, 50f);
                    //Debug.DrawRay(vertices[j], randomVertex, Color.magenta, 50f);
                    RaycastHit hit;
                    if (Physics.Raycast(randomVertex, vertex, out hit))
                    {
                        //Debug.DrawLine(randomVertex, vertex, Color.cyan, 100f);//CAUTION: Giant web of DOOM appears beware!
                        GameObject go = Instantiate(assets[assetID], hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                        go.transform.parent = transform;
                    }
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = gameObject.GetComponent<MeshRenderer>().bounds.center;
        float radius = gameObject.GetComponent<MeshRenderer>().bounds.extents.magnitude;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(center, radius);
    }
}
