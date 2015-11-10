using UnityEngine;
using System.Collections;

public class OrbitCharacter : MonoBehaviour {
    public Transform target;
    public float maxViewDistance = 15f;
    public float minViewDistance = 3f;
    float distance = 10.0f;
    float correctedDistance;
    float desiredDistance;
    float currentDistance;
    bool isCorrected = false;
    public float parentHeight = 1.5f; //height of Player Character.

    float xSpeed = 250.0f;
    float ySpeed = 120.0f;

    public float yMax = 10;
    public float yMin = -30;

    int zoomRate = 30;

    private float x = 0.0f;
    private float y = 0.0f;

    void Start() {
        currentDistance = distance;
        desiredDistance = distance;
        correctedDistance = distance;

        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate() {
        if (target) {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
                x += Input.GetAxis("Mouse X");// * xSpeed * 0.02f;
                y -= Input.GetAxis("Mouse Y");// * ySpeed * 0.02f;
            }

            y = Mathf.Clamp(y, yMin, yMax);//Restrain 'Y' between MAX and MIN values.

            var rotation = Quaternion.Euler(y, x, 0);

            #region Zoom in-out
            desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);//Calculates the desired distance from the Character.
            desiredDistance = Mathf.Clamp(desiredDistance, minViewDistance, maxViewDistance);//Restrain the distance between MAX and MIN values.
            correctedDistance = desiredDistance;
            #endregion

            #region Obstructed view fixed
            Vector3 position = target.position - (rotation * Vector3.forward * desiredDistance);//
            RaycastHit hitInfo;
            Vector3 cameraTargetPosition = new Vector3(target.position.x, target.position.y + parentHeight, target.position.z);
            isCorrected = false;
            if (Physics.Linecast(cameraTargetPosition, position, out hitInfo)) {
                position = hitInfo.point;
                correctedDistance = Vector3.Distance(cameraTargetPosition, position);
                isCorrected = true;
            }
            currentDistance = !isCorrected || correctedDistance > currentDistance ? Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomRate) : correctedDistance;
            #endregion

            if (Input.GetMouseButton(1)) {
                target.rotation = Quaternion.Euler(0, x, 0);
            }

            position = rotation * new Vector3(0.0f, parentHeight, -currentDistance) + target.position;
            
            transform.rotation = rotation;
            transform.position = position;
        }
    }
}
