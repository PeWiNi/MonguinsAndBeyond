using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnvironmentPlacement : MonoBehaviour
{
    public enum Placement // your custom enumeration
    {
        Random,
        Segmented,
        None
    };
    public Placement placementState = Placement.None;//Default None.

    public List<GameObject> environmentAssetsPool;
    [Range(0f, 5000f)]
    public int maxNumberOfAssets;

    private Mesh mesh;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        if (placementState == Placement.Random)
            RandomizeEnvironment();
        if (placementState == Placement.Segmented)
            SegmentedPlacement(0);
    }

    /// <summary>
    /// Create randomly placed environment assets.
    /// </summary>
    void RandomizeEnvironment()
    {
        Vector3[] vertices = mesh.vertices;
        int amountWeNeed = 0;
        //for (int i = 0; i < maxNumberOfAssets; i++)
        while (amountWeNeed < maxNumberOfAssets)
        {
            //Get a random object from the environmentAssetsPool and assign it to the 'go' GameObject.
            GameObject go = environmentAssetsPool[(int)Random.Range(0f, environmentAssetsPool.Count)];
            //Access a random vertex from the vertices of the mesh.
            Vector3 randomVertex = vertices[Random.Range(0, vertices.Length)];
            //Get a point from the Random.onUnitSphere and apply a distance from the origin of that point (this is where the Ray later will have its origin).
            Vector3 direction = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
            //Ensures that we do not have a direction thats below the 'surface' area.
            while (direction.y <= 5f)
            {
                direction = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
            }
            //Debug.DrawLine(direction, randomVertex, Color.magenta, 15, false);
            RaycastHit hit;
            if (Physics.Raycast(direction, randomVertex, out hit))
            {
                //Instantiate the 'go' GameObject.
                Instantiate(go, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
                amountWeNeed++;
            }
        }
    }

    /// <summary>
    /// Based on the following; 
    /// 1) Divison (of the mesh, based on squares/circles). 
    /// 2) Percentage(%) Fill Amount (the amount you want to fill this segment). 
    /// 3) Location within segment (where do you want to place them, middle, upper corner etc).
    /// The map will be filled with environmental assets at the geiven Location based on values.
    /// </summary>
    /// <param name="environmentAsset"></param>
    void SegmentedPlacement(int environmentAsset)
    {
        Vector3[] vertices = mesh.vertices;
        int amountWeNeed = 0;
        //for (int i = 0; i < maxNumberOfAssets; i++)
        while (amountWeNeed < vertices.Length)
        {
            //Get the environment asset you want to fill this segment with.
            GameObject go = environmentAssetsPool[environmentAsset];
            //Access a random vertex from the vertices of the mesh.
            Vector3 randomVertex = vertices[amountWeNeed];
            //Debug.DrawLine(direction, randomVertex, Color.magenta, 15, false);
            RaycastHit hit;
            if (Physics.Raycast(Vector3.up * 20f, randomVertex, out hit))
            {
                Debug.DrawLine(Vector3.up * 20f, randomVertex, Color.blue, 100, false);
                //Instantiate the 'go' GameObject.
                Instantiate(go, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
            }
            amountWeNeed++;
        }
    }
}
