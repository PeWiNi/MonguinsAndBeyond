using UnityEngine;
using System.Collections;

public class CharacterCamera : MonoBehaviour {
    public Transform target;
    public float maxViewDistance = 15f;
    public float minViewDistance = 3f;
    public float parentHeight = 1.5f; //height of Player Character.

    // Correction variables
    float distance = 10.0f;
    float correctedDistance;
    float desiredDistance;
    public float currentDistance;
    bool isCorrected = false;

    // Camera Speed
    float xSpeed = 180.0f;
    float ySpeed = 120.0f;
    float rotationDampening = 3f;

    // Vertical restictions on camera movement
    public float yMax = 70;
    public float yMin = -10;

    int zoomRate = 30;

    private float x = 0.0f;
    private float y = 0.0f;
    
    public Vector3 rotate;

    void Start() {
        currentDistance = distance;
        desiredDistance = distance;
        correctedDistance = distance;

        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate() {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) { // Move camera
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        } else if (Input.GetAxis("Vertical") != 0f)// || Input.GetAxis("Horizontal") != 0f) // Reset rotation when moving
            if (!GetComponentInParent<PlayerStats>().isStunned) { // No more spinny spinny camera
                var targetRotationAngle = target.eulerAngles.y;
                var currentRotationAngle = transform.eulerAngles.y;
                x = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, Time.deltaTime * rotationDampening);
        }
        //} else if (Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal") != 0f) // Reset rotation when moving
        //    if (!GetComponentInParent<PlayerStats>().isStunned) { // No more spinny spinny camera
        //        var targetRotationAngle = target.eulerAngles.y;
        //        var currentRotationAngle = transform.eulerAngles.y;
        //        x = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, Time.deltaTime * rotationDampening);
        //}

        y = Mathf.Clamp(y, yMin, yMax);//Restrain 'Y' between MAX and MIN values.

        var rotation = Quaternion.Euler(y, x, 0);

        #region Zoom in-out
        desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);//Calculates the desired distance from the Character.
        desiredDistance = Mathf.Clamp(desiredDistance, minViewDistance, maxViewDistance);//Restrain the distance between MAX and MIN values.
        correctedDistance = desiredDistance;
        #endregion

        #region Obstructed view fixed
        Vector3 position = target.position - (rotation * Vector3.forward * desiredDistance);
        RaycastHit hitInfo;
        Vector3 cameraTargetPosition = new Vector3(target.position.x, target.position.y + parentHeight, target.position.z);
        isCorrected = false;
        if (Physics.Linecast(cameraTargetPosition, position, out hitInfo, ~(1 << 8))) { // Bit Values of doom! (1 << 8) means only '8th layer', ~ means 'ignore', | is 'and'
            position = hitInfo.point;
            //Debug.DrawLine(cameraTargetPosition, position, Color.blue, 100f);
            correctedDistance = Vector3.Distance(cameraTargetPosition, position);
            isCorrected = true;
        }
        currentDistance = !isCorrected || correctedDistance > currentDistance ? Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomRate) : correctedDistance;
        #endregion

        rotate = new Vector3(y, x, 0);

        position = rotation * new Vector3(0.0f, parentHeight, -currentDistance) + target.position;

        transform.rotation = rotation;
        transform.position = position;
    }
}
