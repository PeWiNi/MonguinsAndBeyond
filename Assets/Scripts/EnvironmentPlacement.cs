using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnvironmentPlacement : MonoBehaviour
{

    public GameObject segmentMap;
    public List<GameObject> environmentAssetsPool;
    [Range(0f, 500f)]
    public int maxNumberOfAssets;

    private Mesh mesh;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        RandomizeEnvironment();
    }

    void RandomizeEnvironment()
    {
        //Transform vertices from Local Space to World Space.
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 norm = transform.TransformDirection(mesh.normals[i]);
            Vector3 vert = transform.TransformPoint(mesh.vertices[i]);
            //Debug.DrawRay(vert, norm * 1, Color.red);
        }
        //Get all the vertices of the mesh.
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];
        Bounds bounds = mesh.bounds;
        int h = 0;
        while (h < uvs.Length)
        {
            uvs[h] = new Vector2(vertices[h].x / bounds.size.x, vertices[h].z / bounds.size.x);
            h++;
        }
        mesh.uv = uvs;
        print("Number of vertices = " + h);

        for (int i = 0; i < maxNumberOfAssets; i++)
        {
            //Get a random object from the environmentAssetsPool and assign it to the 'go' GameObject.
            GameObject go = environmentAssetsPool[(int)Random.Range(0f, environmentAssetsPool.Count)];
            //Place the 'go' GameObject at a random location within the bounds of the segment.
            Vector3 goPosition = vertices[Random.Range(0, vertices.Length)];
            //Randomize the rotation to diversify the environment.
            Quaternion goRotation = Quaternion.EulerAngles(0f, Random.Range(-180f, 180f), 0f);
            //Instantiate the 'go' GameObject
            Instantiate(go, goPosition, goRotation);
        }
    }
}
