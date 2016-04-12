using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class RubberTree : NetworkBehaviour
{
    //[Tooltip("The force applied to the Player")]
    //public float thrust = 0f;
    //[Tooltip("The SphereCollider used for determine the triggerRadius area")]
    //public SphereCollider crownTriggerCollider = null;
    //[Tooltip("The landing spot which players will be thrown towards")]
    //public Vector3 theLandingSpotposition;
    //[Tooltip("The angle of the rubber tree")]
    //public float shootingAngle;
    //[Tooltip("The range for which players will be thrown")]
    //public float shootingRange;
    //[Tooltip("The original rotation of the rubber tree\nUsed when the player no longer interacts with the tree")]
    //public Quaternion originalRotation = Quaternion.identity;
    //[Tooltip("The xMarksTheSpot projector")]
    //public GameObject landingspotProjector;

    //Mouse Cursors + states
    //public Texture2D cursorFreeHand;
    //public Texture2D cursorGrab;
    //public CursorMode cursorMode = CursorMode.Auto;
    //public Vector2 hotSpot = Vector2.zero;
    //public bool isHoveringTrap = false;//Hovering over trap.
    //public bool isHoldingDownTrap = false;//Holding LMB Down.

    public GameObject parent;

    // Use this for initialization
    void Start()
    {
        //if (crownTriggerCollider == null)
        //    crownTriggerCollider = GetComponent<SphereCollider>();
        //originalRotation = transform.rotation;
    }

    //void Update()
    //{
        //TrapDirectionCursorState();
    //}

    //private void TrapDirectionCursorState()
    //{
    //    if (Input.GetKey(KeyCode.Mouse0) && isHoveringTrap && !isHoldingDownTrap)
    //    {
    //        isHoldingDownTrap = true;
    //        Cursor.SetCursor(cursorGrab, hotSpot, cursorMode);
    //    }
    //    else if (!(Input.GetKey(KeyCode.Mouse0) && isHoveringTrap))
    //    {
    //        isHoldingDownTrap = false;
    //    }
    //    if (isHoldingDownTrap && player != null)
    //    {
    //        float dist = Vector3.Distance(player.transform.position, transform.position);
    //        Vector3 circlePos = CircleStuff(transform.position, 10, dist);

    //        Vector3 circleDir = circlePos - transform.position;
    //        Vector3 targetDir = (player.transform.position - new Vector3(0f, player.GetComponentInChildren<CharacterCamera>().parentHeight, 0f)) - transform.position;
    //        Vector3 newCDir = Vector3.RotateTowards(transform.forward, circleDir, 2 * Time.deltaTime, 0.0F); // Rotates the thing correctly in the X-axis (using distance)
    //        Vector3 newTDir = Vector3.RotateTowards(transform.forward, targetDir, 2 * Time.deltaTime, 0.0F); // Rotates the thing correctly in the Y-axis (following player)
    //        transform.rotation = Quaternion.Euler(Quaternion.LookRotation(newCDir).eulerAngles.x, Quaternion.LookRotation(newTDir).eulerAngles.y, 0f);

    //        this.isUnderConstruction = true;
    //        if (Input.GetKeyDown(KeyCode.Mouse1))
    //        {
    //            SlingshotPlayer(dist);
    //        }
    //    }
    //    else
    //    {
    //        this.isUnderConstruction = false;
    //        transform.rotation = originalRotation;
    //    }
    //}

    /// <summary>
    /// Method for transforming a distance to 
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <param name="distance"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    //Vector3 CircleStuff(Vector3 center, float radius, float distance, float min = 0, float max = 10)
    //{
    //    var ang = Mathf.Clamp(distance, 0, 10) / 10 * -90;
    //    //ang += 90;
    //    Vector3 pos;
    //    pos.x = center.x + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
    //    pos.y = center.y + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
    //    pos.z = center.z;
    //    return pos;
    //}

    /// <summary>
    /// Remap values.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="from1"></param>
    /// <param name="to1"></param>
    /// <returns></returns>
    //public float Remap(float value, float from1, float to1, float from2, float to2)
    ////public float Remap(float value, float from1, float to1)
    //{
    //    //    float from2 = 2f;//C
    //    //    float to2 = 5 * from2;//D
    //    return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    //}

    /// <summary>
    /// Slingshot the player.
    /// </summary>
    /// <param name="dist"></param>
    //private void SlingshotPlayer(float dist)
    //{
    //    this.isHoldingDownTrap = false;
    //    this.isUnderConstruction = false;
    //    this.isTriggered = true;
    //    shootingAngle = Vector3.Angle(transform.up, Vector3.up);
    //    print("dist = " + dist);
    //    print("shootingAngle = " + shootingAngle);
    //    shootingAngle = Remap(shootingAngle, 0f, 90f, 0f, 60f);//Y = (X-A)/(B-A) * (D-C) + C, ShootingAngle = X. A = 0f, B = 60f.
    //    print("shootingAngle After Remap = " + shootingAngle);
    //    //shootingRange = Remap(dist, 0f, 10f, 0f, 30f);//Y = (X-A)/(B-A) * (D-C) + C, Dist = X. A = 0f, B = 30f.
    //    //print("shootingRange After Remap = " + Remap(dist, 0f, 10f, 0f, 30f));
    //    shootingRange = DistperDegree(shootingAngle);
    //    print("shootingRange After DistValueRemap = " + DistperDegree(shootingAngle));

    //    theLandingSpotposition = transform.position + (-transform.up * shootingRange);
    //    theLandingSpotposition.y = 1f;
    //    Debug.DrawLine(transform.position, theLandingSpotposition, Color.yellow, 100f);

    //    thrust = Mathf.Sqrt((shootingRange * Physics.gravity.magnitude) / Mathf.Sin(2f * shootingAngle * Mathf.Deg2Rad));//Determine the launch velocity.
    //    thrust = Mathf.Clamp(thrust, 0f, 10f);//We need to clamp the thrust between 0f and 10f, otherwise we will get INSANE LAUNCH VELOCITY because of the angle.
    //    print("Thrust = " + thrust);
    //    player.gameObject.GetComponent<PlayerBehaviour>().PlayerThrown(thrust, theLandingSpotposition, shootingAngle);
    //    this.isTriggered = false;
    //    ResetValues();
    //}

    //float DistperDegree(float angle)
    //{
    //    float distanceToTravelPerDegree = 0.5f;
    //    return angle * distanceToTravelPerDegree;
    //}

    /// <summary>
    /// Reset the values.
    /// </summary>
    //void ResetValues()
    //{
    //    this.shootingAngle = 0f;
    //    this.shootingRange = 0f;
    //    this.theLandingSpotposition = Vector3.zero;
    //    this.thrust = 0f;
    //    player = null;
    //    isHoldingDownTrap = false;
    //    isHoveringTrap = false;
    //    Cursor.SetCursor(null, Vector2.zero, cursorMode);
    //}

    void OnTriggerEnter(Collider collider)
    {
        //if (_collider.tag == "Player" && !this.isTriggered && !this.isUnderConstruction && player == null)
        //player = _collider.gameObject;
        if (collider.tag == "Player")
        {
            collider.gameObject.GetComponent<PlayerBehaviour>().isThrown = true;
            parent.GetComponent<Animator>().SetTrigger("IsTriggered");
        }
    }

    void OnTriggerStay(Collider collider)
    {
        //if (_collider.tag == "Player" && !this.isTriggered && !this.isUnderConstruction && player == null)
        //    player = _collider.gameObject; 
        if (collider.tag == "Player")
        {
            collider.gameObject.GetComponent<PlayerBehaviour>().isThrown = true;
        }
    }

    //void OnTriggerExit(Collider collider)
    //{
        /*If the player that was player would leave or not interact with the trap anymore, set it to null so another can take possesion of it*/
        //if (_collider.gameObject == player && !this.isHoldingDownTrap && !this.isUnderConstruction)
        //    player = null; 
    //}

    //void OnMouseOver()
    //{
    //    if (!isHoldingDownTrap && player != null)
    //    {
    //        isHoveringTrap = true;
    //        Cursor.SetCursor(cursorFreeHand, hotSpot, cursorMode);
    //    }
    //}

    //void OnMouseExit()
    //{
    //    if (!isHoldingDownTrap)
    //    {
    //        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    //        isHoveringTrap = false;
    //        player = null;
    //    }
    //}
}
