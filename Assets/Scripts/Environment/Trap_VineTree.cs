using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Trap_VineTree : Trap
{
    public GameObject player;
    [Tooltip("The force applied to the Player")]
    public float thrust = 0f;
    [Tooltip("The SphereCollider used for determine the triggerRadius area")]
    public SphereCollider crownTriggerCollider = null;
    [Tooltip("The landing spot which players will be thrown towards")]
    public Vector3 theLandingSpotposition;
    [Tooltip("The angle of the rubber tree")]
    public float shootingAngle;
    [Tooltip("The range for which players will be thrown")]
    public float shootingRange;
    //[Tooltip("The distance between the landing spot and the rubber tree")]
    //public float distancePlayersWillBeThrown;
    [Tooltip("The original rotation of the rubber tree\nUsed when the player no longer interacts with the tree")]
    public Quaternion originalRotation = Quaternion.identity;
    [Tooltip("The xMarksTheSpot projector")]
    public GameObject landingspotProjector;

    //Mouse Cursors + states
    public Texture2D cursorFreeHand;
    public Texture2D cursorGrab;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;
    public bool isHoveringTrap = false;//Hovering over trap.
    public bool isHoldingDownTrap = false;//Holding LMB Down.

    // Use this for initialization
    void Start()
    {
        if (crownTriggerCollider == null)
            crownTriggerCollider = GetComponent<SphereCollider>();
        originalRotation = transform.rotation;
    }

    void Update()
    {
        TrapDirectionCursorState();
    }

    private void TrapDirectionCursorState()
    {
        if (Input.GetKey(KeyCode.Mouse0) && isHoveringTrap && !isHoldingDownTrap)
        {
            isHoldingDownTrap = true;
            Cursor.SetCursor(cursorGrab, hotSpot, cursorMode);
        }
        else if (!(Input.GetKey(KeyCode.Mouse0) && isHoveringTrap))
        {
            isHoldingDownTrap = false;
            //Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }
        if (isHoldingDownTrap && player != null)
        {
            /*
            // Buttom to buttom direction
            Vector3 targetDir = (player.transform.position - new Vector3(0f, player.GetComponentInChildren<CharacterCamera>().parentHeight, 0f)) - transform.position;
            // Measure distance
            float dist = Vector3.Distance(player.transform.position, transform.position);
            // Bend it towards direction of player (target direction modified by distance (Figure a smart way of assigning a point in space somewhere below the tree for it to point towards))
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir - (Vector3.up * (dist - 1)), 2 * Time.deltaTime, 0.0F);//Original
            Debug.DrawRay(transform.position, newDir, Color.red);
            transform.rotation = Quaternion.LookRotation(newDir);
            //print(dist);
            

            Vector3 targetDir = player.transform.position - transform.position;
            //float step = player.GetComponent<PlayerStats>().speed * Time.deltaTime;
            float step = Vector3.Distance(player.transform.position, transform.position);
            Vector3 newDir = Vector3.RotateTowards(transform.position, targetDir, step, 0.0F);//Original
            transform.rotation = Quaternion.LookRotation(-newDir);
            */
            float dist = Vector3.Distance(player.transform.position, transform.position);
            Vector3 circlePos = CircleStuff(transform.position, 10, dist);
            //Debug.DrawLine(circlePos, transform.position, Color.blue); // Does the correct vertical angle, but doesn't follow player direction ._.

            Vector3 circleDir = circlePos - transform.position;
            Vector3 targetDir = (player.transform.position - new Vector3(0f, player.GetComponentInChildren<CharacterCamera>().parentHeight, 0f)) - transform.position;
            Vector3 newCDir = Vector3.RotateTowards(transform.forward, circleDir, 2 * Time.deltaTime, 0.0F); // Rotates the thing correctly in the X-axis (using distance)
            Vector3 newTDir = Vector3.RotateTowards(transform.forward, targetDir, 2 * Time.deltaTime, 0.0F); // Rotates the thing correctly in the Y-axis (following player)
            transform.rotation = Quaternion.Euler(Quaternion.LookRotation(newCDir).eulerAngles.x, Quaternion.LookRotation(newTDir).eulerAngles.y, 0f);
            //transform.rotation = Quaternion.LookRotation(newCDir, newTDir);

            this.isUnderConstruction = true;
            /*Set state of the trap to Assemble*/
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                this.isHoldingDownTrap = false;
                this.isUnderConstruction = false;
                this.isAssembled = true;
                print("Assembled!");
                //shootingRange = Mathf.Clamp(dist, 0f, 10f) / 10;//This is used for the projection.
                //theLandingSpotposition = transform.position + (-transform.up * DistanceMapping(shootingRange));

                shootingAngle = Vector3.Angle(transform.up, Vector3.up);
                print("dist = " + dist);
                print("shootingAngle = " + shootingAngle);
                shootingRange = Remap(dist, 0f, 10f);//Y = (X-A)/(B-A) * (D-C) + C, Dist = X. A = 0f, B = 10f.
                print("shootingRange = " + shootingRange);
                theLandingSpotposition = transform.position + (-transform.up * shootingRange);
                theLandingSpotposition.y = 1f;
                Debug.DrawLine(transform.position, theLandingSpotposition, Color.yellow, 100f);
                landingspotProjector = (GameObject)Instantiate(Resources.Load("Prefabs/XMarksTheSpot_Projector") as GameObject, theLandingSpotposition,
                    Quaternion.Euler(new Vector3(90, 0f, 0f)));
                //Quaternion.Euler(new Vector3(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)));
            }
        }
        else if (!this.isAssembled)
        {
            this.isUnderConstruction = false;
            transform.rotation = originalRotation;
        }
    }

    Vector3 CircleStuff(Vector3 center, float radius, float distance, float min = 0, float max = 10)
    {
        var ang = Mathf.Clamp(distance, 0, 10) / 10 * -90;
        //ang += 90;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }

    //public float Remap(float value, float from1, float to1, float from2, float to2)
    /// <summary>
    /// Remap the distance.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="from1"></param>
    /// <param name="to1"></param>
    /// <returns></returns>
    public float Remap(float value, float from1, float to1)
    {
        float from2 = 2f;//C
        float to2 = 5 * from2;//D
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    //float DistanceMapping(float angle)
    //{
    //    float max = 0.8f;
    //    float min = 1f;
    //    float maxDistance = 30f;
    //    float minDistance = maxDistance / 5f;// 1/5 of the maxDistance.
    //    return (angle - min) / (minDistance - min) * (maxDistance - max) + max;
    //}

    void OnTriggerEnter(Collider _collider)
    {
        if (_collider.tag == "Player" && !this.isAssembled && !this.isTriggered && !this.isUnderConstruction)
            player = _collider.gameObject;
        if (_collider.tag == "Player" && this.isAssembled)
        {
            this.isTriggered = true;
            this.isAssembled = false;
            //We search for all Players in the triggerRadius to determine who should be thrown.
            Collider[] playersWithinRange = Physics.OverlapSphere(crownTriggerCollider.gameObject.transform.position, 10f);
            foreach (Collider col in playersWithinRange)
            {
                if (col.tag == "Player")
                {
                    thrust = Mathf.Sqrt((shootingRange * Physics.gravity.magnitude) / Mathf.Sin(2f * shootingAngle * Mathf.Deg2Rad));//Determine the launch velocity.
                    print("Thrust = " + thrust);
                    Destroy(landingspotProjector);
                    _collider.gameObject.GetComponent<PlayerBehaviour>().PlayerThrown(thrust, theLandingSpotposition, shootingAngle);
                }
            }
            this.isTriggered = false;
        }
    }

    void OnTriggerExit(Collider _collider)
    {
        /*If the player that was player would leave, set it to null so another can take possesion of it*/
        if (_collider.gameObject == player)
        {
            player = null;
        }
    }

    void OnMouseEnter()
    {
        Cursor.SetCursor(cursorFreeHand, hotSpot, cursorMode);
        isHoveringTrap = true;
    }

    void OnMouseExit()
    {
        if (!isHoldingDownTrap)
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
            isHoveringTrap = false;
        }
    }

    void OnDrawGizmos()
    {
        if (crownTriggerCollider != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(crownTriggerCollider.bounds.center, 5f);
        }
    }
}
