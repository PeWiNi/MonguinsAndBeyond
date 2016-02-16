using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnvironmentPlacement : MonoBehaviour
{

    public GameObject segmentMap;
    public List<GameObject> environmentAssetsPool;
    [Range(0f, 5000f)]
    public int maxNumberOfAssets;

    private Mesh mesh;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        RandomizeEnvironment();
    }

    /// <summary>
    /// Based on the bounds of the mesh, create randomly placed environment assets.
    /// </summary>
    void RandomizeEnvironment()
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < maxNumberOfAssets; i++)
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
            Debug.DrawLine(direction, randomVertex, Color.magenta, 15, false);
            RaycastHit hit;
            if (Physics.Raycast(direction, randomVertex, out hit))
            {
                //Debug.DrawLine(hit.point, hit.point + hit.point.normalized, Color.blue, 100, false);
                //Debug.DrawLine(hit.point, direction, Color.blue, 100, false);
                //Debug.DrawLine(hit.point, hit.normal, Color.green, 100, false);
                //Instantiate the 'go' GameObject
                Instantiate(go, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));//The FromToRotation(Vector3.up, hit.normal) ensures we align the 'go' GameObject along the surface of the mesh.
            }
        }
    }
}
