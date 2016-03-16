using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OuterAreaPlacement : MonoBehaviour
{
    [Tooltip("The asset going to be used")]
    public GameObject asset = null;
    [Tooltip("Amount of the asset that is going to be used")]
    public int numberOfAsset = 0;
    [Tooltip("The LayerMask we wanna raycast against")]
    public LayerMask mask = -1;

    Bounds bounds;
    Vector3 v3Center = Vector3.zero;
    Vector3 v3Extents = Vector3.zero;
    Vector3 vTopLeft;
    Vector3 vTopRight;
    Vector3 vBottomLeft;
    Vector3 vBottomRight;

    // Use this for initialization
    void Start()
    {
        bounds = GetComponent<MeshFilter>().sharedMesh.bounds;
        v3Center = bounds.center;
        v3Extents = bounds.extents;
        CalcPositons();
        OuterArea();
    }

    //void Update()
    //{
    //    CalcPositons();
    //    DrawBox();
    //}

    /// <summary>
    /// Create a triangle shaped relation between the rubber trees.
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="numberOfAsset"></param>
    /// <param name="startingPosition"></param>
    void AddRubberTreeSection_TriangleShape(GameObject asset, int numberOfAsset, Vector3 startingPosition)
    {
        Vector3[] points = new Vector3[3];
        float ypos = transform.position.y;//MUST be the on same height & normal direction as the Terrain.
        float xpos = startingPosition.x;
        //float zpos = -xpos;
        float zpos = startingPosition.z;
        for (int i = 0; i < numberOfAsset; i++)
        {
            zpos = -zpos + xpos;
            xpos = zpos - xpos;
            Vector3 pos = new Vector3(xpos, ypos, zpos);
            GameObject go = Instantiate(asset, pos, Quaternion.identity) as GameObject;
            points[i] = go.transform.position;
        }
        Debug.DrawLine(points[0], points[1], Color.blue, 100f);
        Debug.DrawLine(points[1], points[2], Color.blue, 100f);
        Debug.DrawLine(points[2], points[0], Color.blue, 100f);
    }

    /// <summary>
    /// Calculate the positions based on extents of the bounds.
    /// </summary>
    void CalcPositons()
    {
        vTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);
        vTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);
        vBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);
        vBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);
        vTopLeft = transform.TransformPoint(vTopLeft);
        vTopRight = transform.TransformPoint(vTopRight);
        vBottomLeft = transform.TransformPoint(vBottomLeft);
        vBottomRight = transform.TransformPoint(vBottomRight);
    }

    void DrawBox()
    {
        Debug.DrawLine(v3Center, (vTopLeft + vTopRight) / 2, Color.red);
        Debug.DrawLine(v3Center, (vBottomLeft + vBottomRight) / 2, Color.red);
        Debug.DrawLine(v3Center, (vTopLeft + vBottomLeft) / 2, Color.red);
        Debug.DrawLine(v3Center, (vTopRight + vBottomRight) / 2, Color.red);

        //Cube Bounds (box).
        Vector3 force = Vector3.Project((((vBottomLeft + vBottomRight) / 2) * (Mathf.Sqrt(2)) / 2), (vBottomLeft + vBottomRight) / 2);//Vector3 projection - to the left side.
        Vector3 force2 = Vector3.Project((((vBottomLeft + vTopLeft) / 2) * (Mathf.Sqrt(2)) / 2), (vBottomLeft + vTopLeft) / 2);//Vector3 projection - to the bottom side.
        Vector3 center = (force + force2);//Vector3 projection - Center.
        Debug.DrawRay(v3Center, force, Color.yellow);
        Debug.DrawRay(v3Center, force2, Color.yellow);
        Debug.DrawRay(force, force2, Color.magenta);
        Debug.DrawRay(force2, force, Color.cyan);

        //Leftover parts.
        Debug.DrawRay((vBottomLeft + vBottomRight) / 2, (vBottomLeft + vTopLeft) / 2, Color.magenta);
        Debug.DrawRay((vBottomLeft + vTopLeft) / 2, (vBottomLeft + vBottomRight) / 2, Color.cyan);
    }

    /// <summary>
    /// Fill the inner areas with stuff.
    /// </summary>
    public void ConfinedArea()
    {
        Vector3 force = Vector3.Project((((vBottomLeft + vBottomRight) / 2 + (vBottomLeft + vTopLeft) / 2) * Mathf.Sqrt(2)) / 2, (vBottomLeft + vBottomRight) / 2);//Towards Bottom.
        Vector3 force2 = Vector3.Project((((vBottomLeft + vBottomRight) / 2 + (vBottomLeft + vTopLeft) / 2) * Mathf.Sqrt(2)) / 2, (vBottomLeft + vTopLeft) / 2);//Towards Left.
        Vector3 force3 = Vector3.Project((((vTopLeft + vTopRight) / 2 + (vBottomRight + vTopRight) / 2) * Mathf.Sqrt(2)) / 2, (vTopLeft + vTopRight) / 2);//Towards Top.
        Vector3 center = ((force + force2) + v3Center) / 2;//Center - Half length diagonal within square.

        #region Corner Sphere Areas
        //Create Spheres
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.localScale *= Vector3.Distance(force, force2) / 4;
        //go.transform.position = ((force + force2) - center);
        go.transform.position = (center + (force + force2)) / 2;

        Vector3 point = v3Center + -Vector3.right * (Vector3.Distance(v3Center, force2) / 4) * 3f;
        Debug.DrawLine(go.transform.position + -Vector3.right * go.transform.localScale.x / 2, point, Color.red, 100f);
        Debug.DrawLine(go.transform.position + Vector3.right * go.transform.localScale.x / 2, point, Color.red, 100f);

        //Create 3 other Sphere areas all identical but with different placements.
        GameObject nextSphere = go;
        Vector3 newSpherePosition = nextSphere.transform.position;
        for (int i = 0; i < 3; i++)
        {
            newSpherePosition = new Vector3(newSpherePosition.z, newSpherePosition.y, -newSpherePosition.x);//Set the positions perpendicular to the previous location.
            GameObject.Instantiate(nextSphere, newSpherePosition, Quaternion.identity);
        }
        #endregion
    }

    /// <summary>
    /// Fill out the Outer areas with stuffs (Twees).
    /// </summary>
    public void OuterArea()
    {
        Vector3 force = Vector3.Project((((vBottomLeft + vBottomRight) / 2 + (vBottomLeft + vTopLeft) / 2) * Mathf.Sqrt(2)) / 2, (vBottomLeft + vBottomRight) / 2);
        Vector3 force2 = Vector3.Project((((vBottomLeft + vBottomRight) / 2 + (vBottomLeft + vTopLeft) / 2) * Mathf.Sqrt(2)) / 2, (vBottomLeft + vTopLeft) / 2);
        Vector3 betweenForces = force + force2;
        Vector3 straightLeft = (vBottomLeft + vTopLeft) / 2;
        Vector3 straightDown = (vBottomLeft + vBottomRight) / 2;

        GameObject allOuterAreas = new GameObject();
        allOuterAreas.transform.localScale = new Vector3(Vector3.Distance(v3Center, straightLeft), 10f, Vector3.Distance(v3Center, straightDown));//The size of the squares.
        allOuterAreas.transform.position = ((straightLeft + straightDown) / 2) + v3Center;//Center it in the down left area.

        //Create Vertical fill outer area.
        int amountV = this.numberOfAsset;
        while (amountV > 0)
        {
            Vector3 newVector = new Vector3(Random.Range(straightLeft.x, force2.x), transform.position.y, Random.Range(straightLeft.z, betweenForces.z));
            RaycastHit hit;
            Ray ray = new Ray(Vector3.up + newVector, -Vector3.up);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask.value))
            {
                Debug.DrawRay(ray.origin, ray.direction * 10000, Color.yellow, 100f);
                print("V: Hit name = " + hit.transform.name);
                //Debug.DrawLine(Vector3.up + newVector, -Vector3.up + newVector, Color.blue, 100f);
                GameObject go = GameObject.Instantiate(asset, newVector, Quaternion.Euler(new Vector3(0f, Random.Range(0f, 180f), 0f))) as GameObject;
                float height = Random.Range(go.transform.localScale.y, go.transform.localScale.y * 1.25f);
                float width = go.transform.localScale.x * height;
                go.transform.localScale = new Vector3(width, height, width);
                go.transform.parent = allOuterAreas.transform;
            }
            else Debug.DrawLine(Vector3.up * 10 + newVector, -Vector3.up * 10 + newVector, Color.red, 100f);
            amountV--;
        }

        //Create Horienzontal fill outer area.
        int amountH = this.numberOfAsset;
        while (amountH > 0)
        {
            Vector3 newVector = new Vector3(Random.Range(straightDown.x, betweenForces.x), transform.position.y, Random.Range(straightDown.z, force.z));
            RaycastHit hit;
            Ray ray = new Ray(Vector3.up + newVector, -Vector3.up);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask.value))
            {
                Debug.DrawRay(ray.origin, ray.direction * 10000, Color.yellow, 100f);
                print("H: Hit name = " + hit.transform.name);
                //Debug.DrawLine(Vector3.up + newVector, -Vector3.up + newVector, Color.blue, 100f);
                GameObject go = GameObject.Instantiate(asset, newVector, Quaternion.Euler(new Vector3(0f, Random.Range(0f, 180f), 0f))) as GameObject;
                float height = Random.Range(go.transform.localScale.y, go.transform.localScale.y * 1.25f);
                float width = go.transform.localScale.x / height;
                go.transform.localScale = new Vector3(width, height, width);
                go.transform.parent = allOuterAreas.transform;
            }
            else Debug.DrawLine(Vector3.up * 10 + newVector, -Vector3.up * 10 + newVector, Color.red, 100f);
            amountH--;
        }

        //Create 3 other outer areas all identical but with different angles and placements.
        GameObject nextPiece = allOuterAreas;
        Quaternion rotation = Quaternion.Euler(Vector3.zero);
        Vector3 newPiecePosition = nextPiece.transform.position;
        for (int i = 0; i < 3; i++)
        {
            newPiecePosition = new Vector3(newPiecePosition.z, newPiecePosition.y, -newPiecePosition.x);//Set the positions perpendicular to the previous location.
            rotation *= Quaternion.Euler(0, 90, 0);//Turn the rotations 90 Degrees for each new section.
            GameObject.Instantiate(nextPiece, newPiecePosition, rotation);
        }
    }
}
