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

    void RandomizeEnvironment()
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < maxNumberOfAssets; i++)
        {
            //Get a random object from the environmentAssetsPool and assign it to the 'go' GameObject.
            GameObject go = environmentAssetsPool[(int)Random.Range(0f, environmentAssetsPool.Count)];
            //Place the 'go' GameObject at a random location within the bounds of the segment, using Random.UnitSphere as starting position with direction towards the Island mesh.
            Vector3 randomVertice = vertices[Random.Range(0, vertices.Length)];
            Vector3 direction = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
            //Ensures that we do not have a direction thats below the 'surface' area.
            while (direction.y <= 5f)
            {
                direction = Random.onUnitSphere * Vector3.Distance(mesh.bounds.center, mesh.bounds.max * 2);
            }
            //Randomize the rotation to diversify the environment.
            Quaternion goRotation = Quaternion.EulerAngles(0f, Random.Range(-180f, 180f), 0f);
            Debug.DrawLine(direction, randomVertice, Color.magenta, 5, false);
            RaycastHit hit;
            if (Physics.Raycast(direction, randomVertice, out hit))
            {
                //Debug.DrawLine(hit.point, hit.point + hit.point.normalized, Color.blue, 100, false);
                Debug.DrawLine(hit.point, direction, Color.blue, 100, false);
                //Instantiate the 'go' GameObject
                Instantiate(go, hit.point, goRotation);
            }
        }
    }
}
