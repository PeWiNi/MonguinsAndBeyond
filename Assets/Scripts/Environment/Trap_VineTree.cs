using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Trap_VineTree : Trap {
    public GameObject player;
    [Tooltip("The force applied to the Player")]
    public float thrust = 50f;
    [Tooltip("The SphereCollider used for determine the triggerRadius area")]
    public SphereCollider crownTriggerCollider = null;
    [Tooltip("The Direction which players will be thrown")]
    public Vector3 direction;

    //Mouse Cursors + states
    public Texture2D cursorFreeHand;
    public Texture2D cursorGrab;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;
    public bool isHoveringTrap = false;//Hovering over trap.
    public bool isHoldingDownTrap = false;//Holding LMB Down.

    // Use this for initialization
    void Start() {
        if (crownTriggerCollider == null)
            crownTriggerCollider = GetComponent<SphereCollider>();
    }

    void Update() {
        TrapDirectionCursorState();
    }

    private void TrapDirectionCursorState() {
        if (Input.GetKey(KeyCode.Mouse0) && isHoveringTrap && !isHoldingDownTrap) {
            isHoldingDownTrap = true;
            Cursor.SetCursor(cursorGrab, hotSpot, cursorMode);
        } else if (!(Input.GetKey(KeyCode.Mouse0) && isHoveringTrap)) {
            isHoldingDownTrap = false;
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }
        if (isHoldingDownTrap) {
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
        }
    }

    Vector3 CircleStuff(Vector3 center, float radius, float distance, float min = 0, float max = 10) {
        var ang = Mathf.Clamp(distance, 0, 10) / 10 * -90;
        //ang += 90;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }

    void OnTriggerEnter(Collider _collider)
    {
        if (_collider.transform.tag == "Player" && this.isAssembled && !this.isTriggered && !this.isUnderConstruction)
        {
            this.isTriggered = true;
            this.isAssembled = false;
            //We search for all Players in the triggerRadius to determine who should be thrown.
            Collider[] playersWithinRange = Physics.OverlapSphere(transform.position, 2f);
            foreach (Collider col in playersWithinRange)
            {
                if (col.transform.tag == "Player")
                {
                    _collider.gameObject.GetComponent<PlayerBehaviour>().PlayerThrown(thrust);
                }
            }
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
            Gizmos.DrawWireSphere(crownTriggerCollider.bounds.center, 2f);
        }
    }
}
