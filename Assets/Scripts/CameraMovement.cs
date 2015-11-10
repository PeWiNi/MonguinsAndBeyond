using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{

	public Transform parentTransform;
	public float maxViewDistance = 15f;
	public float minViewDistance = 3f;
	public int zoomRate = 30;
	public float maxHorizontalRotation = 60f;//Maximum postive direction allowed for the rotation around the Y axis.
	public float minHorizontalRotation = -60f;//Minimum negative direction allowed for the rotation around the Y axis
    private float minSetHorizontal = 0;
    private float maxSetHorizontal = 0;
    public float maxVerticalRotation = 10f;//Maximum postive direction allowed for the rotation around the X axis.
	public float minVerticalRotation = -30f;//Maximum negative direction allowed for the rotation around the X axis.
	public float parentHeight = 1.5f;//height of Player Character.
	Vector3 currentAngles;
	float distance = 9f;//Starting distance away from the Player.
	float correctedDistance;
	float desiredDistance;
	float currentDistance;
	bool isCorrected = false;

	void Start ()
	{
//		currentAngles = Vector3.forward;
		currentDistance = distance;
		desiredDistance = distance;
		correctedDistance = distance;

        minSetHorizontal = minHorizontalRotation;
        maxSetHorizontal = maxHorizontalRotation;
    }

	void LateUpdate ()
	{
        #region Get angles and Contrain rotation
        //Move camera when left-clicking
        if (Input.GetMouseButton (0)) {
			currentAngles.y += Input.GetAxis ("Mouse X");//Horizontal.
			currentAngles.x -= Input.GetAxis ("Mouse Y");//Vertical.
            //currentAngles.y = Mathf.Clamp(currentAngles.y, minSetHorizontal, maxSetHorizontal);//Restrain 'Y' between MAX and MIN values.
            currentAngles.x = Mathf.Clamp(currentAngles.x, minVerticalRotation, maxVerticalRotation);//Restrain 'X' between MAX and MIN values.
        }
		Quaternion rotation = Quaternion.Euler(currentAngles.x, currentAngles.y, 0f);
        #endregion

        #region Zoom in-out
        desiredDistance -= Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs (desiredDistance);//Calculates the desired distance from the Character.
		desiredDistance = Mathf.Clamp (desiredDistance, minViewDistance, maxViewDistance);//Restrain the distance between MAX and MIN values.
		correctedDistance = desiredDistance;
        #endregion

        #region Obstructed view fixed
        Vector3 position = parentTransform.position - (rotation * Vector3.forward * desiredDistance);//
		RaycastHit hitInfo;
		Vector3 cameraTargetPosition = new Vector3(parentTransform.position.x, parentTransform.position.y + parentHeight, parentTransform.position.z);
		isCorrected = false;
		if(Physics.Linecast(cameraTargetPosition, position, out hitInfo)){
			position = hitInfo.point;
			correctedDistance = Vector3.Distance(cameraTargetPosition, position);
			isCorrected = true;
		}
		currentDistance = !isCorrected || correctedDistance > currentDistance ? Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomRate) : correctedDistance;
        #endregion

        #region Move player according to camera when right-clicking
        if (Input.GetMouseButton(1)) {
            //currentAngles.y = 0;
            //currentAngles.x = 0;

            //Rotate parent
            currentAngles.y += Input.GetAxis("Mouse X");//Horizontal.
            parentTransform.rotation = Quaternion.Euler(0f, currentAngles.y, 0f);

            //Pin camera behind player
            position = parentTransform.position - (parentTransform.forward * currentDistance + new Vector3(0f, -parentHeight / 2, 0f));
            rotation = new Quaternion(0f, 0f, 0f, 0f);
        } else { 
            //Keeps it in its original position
            position = parentTransform.position - (rotation * parentTransform.forward * currentDistance + new Vector3(0f, -parentHeight / 2, 0f));
            //currentAngles.y = 0;
            //currentAngles.x = 0;
            rotation.eulerAngles += new Vector3(0f, parentTransform.rotation.eulerAngles.y, 0f);
        }
        #endregion

        transform.rotation = rotation;
		transform.position = position;
	}
} 