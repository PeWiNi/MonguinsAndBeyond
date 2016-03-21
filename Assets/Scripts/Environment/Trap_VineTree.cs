using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Trap_VineTree : Trap
{
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
    void Start()
    {
        if (crownTriggerCollider == null)
            crownTriggerCollider = GetComponent<SphereCollider>();
    }

    void Update()
    {
        TrapDirectionCursorState();
    }

    private void TrapDirectionCursorState()
    {
        if (Input.GetKey(KeyCode.Mouse0) && isHoveringTrap)
        {
            isHoldingDownTrap = true;
            Cursor.SetCursor(cursorGrab, hotSpot, cursorMode);
            Vector3 targetDir = player.transform.position - transform.position;
            //float step = player.GetComponent<PlayerStats>().speed * Time.deltaTime;
            float step = Vector3.Distance(player.transform.position, transform.position);
            Vector3 newDir = Vector3.RotateTowards(transform.position, targetDir, step, 0.0F);//Original
            transform.rotation = Quaternion.LookRotation(-newDir);
        }
        else
        {
            isHoldingDownTrap = false;
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }
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
