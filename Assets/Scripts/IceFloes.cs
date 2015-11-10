using UnityEngine;
using System.Collections;

public class IceFloes : MonoBehaviour
{

    Quaternion axisAlignRot;
    // Use this for initialization
    void Start()
    {
        axisAlignRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        // Whatever axis you want to align to
        Vector3 worldAxis = new Vector3(0, 1, 0);
        // rotate towards this axis
        Vector3 worldAxisRelative = transform.TransformDirection(worldAxis);
        axisAlignRot = Quaternion.FromToRotation(worldAxisRelative, worldAxis);
        transform.rotation = Quaternion.Slerp(transform.rotation, axisAlignRot * transform.rotation, 1.0f * Time.deltaTime);
    }
}
