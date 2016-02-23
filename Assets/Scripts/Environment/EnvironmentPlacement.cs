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
    public List<GameObject> environmentAssetsPool;
    [Range(0f, 5000f)]
    public int maxNumberOfAssets;
    public GameObject startingPoint;
    public int radius;
    public int assetID;
    public GameObject assetToPlaceRandomly;

    private Mesh mesh;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        if (placementState == Placement.Random)
            RandomPlacement(assetToPlaceRandomly);
        if (placementState == Placement.Area)
            AreaPlacement(this.startingPoint, this.radius, this.maxNumberOfAssets, this.assetID);
        if (placementState == Placement.Fill)
            FillPlacement();
    }

    /// <summary>
    /// Fill environment with assets based on a certain angle.
    /// </summary>
    void FillPlacement()
    {
        Vector3[] vertices = mesh.vertices;
        List<Vector3> verticesList = new List<Vector3>();//Contains normals only with a certain angle.
        float angle = 0f;
        for (int i = 0; i < mesh.normals.Length; i++)
        {
            Vector3 normal = mesh.normals[i];
            if (Vector3.Angle(Vector3.up, normal) <= angle)
            {
                verticesList.Add(vertices[i]);
            }
        }
        int amountWeNeed = 0;
        while (amountWeNeed < maxNumberOfAssets)
        {
            //Get a random object from the environmentAssetsPool and assign it to the 'go' GameObject.
            GameObject go = environmentAssetsPool[(int)Random.Range(0f, environmentAssetsPool.Count)];
            //Access a random vertex from the vertices of the mesh.
            Vector3 randomVertex = verticesList[Random.Range(0, verticesList.Count - 1)];
            //Get a point from the Random.onUnitSphere and apply a distance from the origin of that point (this is where the Ray later will have its origin).
            Vector3 origin = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
            while (origin.y <= 5f || origin.y >= 15f)
            {
                origin = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
            }
            RaycastHit hit;
            if (Physics.Raycast(origin, randomVertex, out hit))
            {
                Instantiate(go, hit.point, Quaternion.identity);//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                amountWeNeed++;
            }
        }
    }

    /// <summary>
    /// Based on the following; 
    /// 1) GameObject Starting Point (this will be the main focus and spawning point for the assets). 
    /// 2) Radius (the boundary around the spawning point, using a sphere). 
    /// 3) Amount Of Assets to spawn within this Radius boundary.
    /// 4) The Asset ID to spawn at random locations within the Radius boundary.
    /// The map will be filled with environmental assets at the geiven Location based on values.
    /// </summary>
    /// <param name="environmentAsset"></param>
    void AreaPlacement(GameObject startingPoint, float radius, int amountOfAssets, int assetID)
    {
        //Get a collection of all colliers touched, because we only want to spawn assets on those.
        Collider[] hitColliders = Physics.OverlapSphere(startingPoint.GetComponent<MeshFilter>().mesh.bounds.center, radius);
        /* 
         * We create a Dictionary<Mesh, Vector3[]> to receive the vertices that will be within the Radius to later acces them randomly.
         * Mesh, is the mesh of the hitColliders GameObjects, Vector3[] will be the vertices that's within the radius on the mesh.
         */
        Dictionary<Mesh, Vector3[]> dictionary = new Dictionary<Mesh, Vector3[]>();
        //Get the environment asset you want to fill this area with.
        GameObject go = environmentAssetsPool[assetID];
        int amountWeNeed = 0;
        while (amountWeNeed < amountOfAssets)
        {
            //Select a random mesh GameObject between the ones that resides in the 'dictionary' variable, to spawn assets on.
            List<Mesh> keyList = new List<Mesh>(dictionary.Keys);
            Mesh mesh = keyList[Random.Range(0, keyList.Count - 1)];
            //Get the vertices of the 'mesh' GameObject.
            Vector3[] vertices = mesh.vertices;
            //Set the origin Vector3 to be the Center of the 'mesh' GameObject.
            Vector3 origin = mesh.bounds.center;
            //Get a random vertex.
            Vector3 randomVertex = vertices[Random.Range(0, vertices.Length)];
            RaycastHit hit;
            if (Physics.Raycast(randomVertex, randomVertex, out hit))//***Should ignore all other layers but the ground***. 
            {
                //Debug.DrawRay(randomVertex, origin, Color.magenta, 100f);
                Instantiate(go, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                amountWeNeed++;
            }
        }
    }

    /// <summary>
    /// Create randomly placed environment asset.
    /// </summary>
    void RandomPlacement(GameObject assetToPlaceRandomly)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 vertex = mesh.vertices[i];
            vertex.x *= transform.localScale.x;
            vertex.y *= transform.localScale.y;
            vertex.z *= transform.localScale.z;
            vertices[i] = vertex;
        }
        for (int j = 0; j < vertices.Length; j++)
        {
            Vector3 randomVertex = vertices[Random.Range(0, vertices.Length)];
            Vector3 vertex = vertices[j];
            RaycastHit hit;
            if (Physics.Raycast(randomVertex, vertex, out hit))
            {
                //Debug.DrawLine(randomVertex, vertex, Color.cyan, 100f);//CAUTION: Giant web of DOOM appears beware!
                Instantiate(assetToPlaceRandomly, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
            }
        }
    }
}
