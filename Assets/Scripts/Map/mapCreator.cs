using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class mapCreator : NetworkBehaviour
{
    public GameObject[] players;

    public GameObject map;
    public GameObject cube;
    //public int[,] ringPartsint=new int[1000,1000];
    public Vector2[,] ringPartsint;

    public int ringsSpawned; //=1;
    public int powCount;
    public float thickness;

    [SyncVar]
    float sinkInit;
    [SyncVar]
    int layersSunk;
    bool startedSinking = false;
    public float timeToNextSink;
    [SyncVar]
    int ringsSunk;
    Vector3 center = new Vector3(0f, 0f, 0f);
    float RingRadius;
    bool sinkingARing;
    public int radius;

    public int centerX = 0;
    public int centerY = 0;
    public int multiplier; //correlates with the cube size 
    MeshGenerator mapData;

    void Awake()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        ringsSpawned = 0;
        map = GameObject.FindGameObjectWithTag("Map");
        timeToNextSink = 15f;

        ringsSunk = 0;
        radius = 11;
        thickness = 8f;
        multiplier = 8;
    }

    // Use this for initialization
    void Start()
    {
    }

    /// <summary>
    /// Modifies the map according to the number players connected.
    /// </summary>
    public bool playerConnected()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        powCount = 0;
        while (Mathf.Pow(2, powCount) <= players.Length)
            powCount++;

        powCount--;//need to go one step back, as the loop above will stop when one step further in the calculation of the pow count 

        //if any of the map rings have sunked previously and we need to spawn new ones. 
        //if (powCount > map.transform.childCount - 1 && powCount > ringsSpawned - ringsSunk)
        if (powCount > map.transform.childCount - 1)
        {
            startSinking();
            // if the number of players requires more map rings than already present, generate them.
            if (!sinkingARing)
            {
                //thickness represents how many extra ring components will the new map ring contain, based on the number of players
                thickness = 8f / Mathf.Pow(2, powCount - 1);

                //increase the number of rings Spawned, substracting the ones that have sunk, to keep track of how many rings are on the map atm.
                ringsSpawned = ringsSpawned - ringsSunk + 1;

                GameObject ringNo = new GameObject(); //create a new Ring parent for the the map parts to be spawned
                ringNo.transform.parent = map.transform; //add the new Ring as a child to the map GameObject
                ringNo.transform.name = ringsSpawned.ToString(); //give it the name of it's ring number
                ringNo.transform.tag = "Ring";

                mapPartBehavior mapPart = ringNo.AddComponent<mapPartBehavior>() as mapPartBehavior;

                mapData = ringNo.AddComponent<MeshGenerator>() as MeshGenerator;
                mapData.AlocateMeshData((int)(thickness + radius) * 2, (int)(thickness + radius) * 2);

                ringDrawing((int)thickness, ringNo);  // need to deal with a float thickness to account for smaller rings.

                //Set the Layer to Ground.
                ringNo.transform.gameObject.layer = LayerMask.NameToLayer("Ground");
                print("RngNo Layer = " + ringNo.transform.gameObject.layer);
                //Remove redundant vertices.
                HashSet<Vector3> noDuplicateVertices = new HashSet<Vector3>();
                Vector3[] vertices = ringNo.GetComponent<MeshFilter>().mesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    noDuplicateVertices.Add(vertices[i]);
                }
                Vector3[] newVertices = new Vector3[noDuplicateVertices.Count];
                noDuplicateVertices.CopyTo(newVertices);
                //Add the new ring to the List of the map.
                map.GetComponent<EnvironmentPlacement>().AddSection(ringNo, newVertices);
            }
            else
            {
                sinkingARing = false;
                int mapRingsNo = map.transform.childCount;
                Transform temp = map.transform.GetChild(mapRingsNo - 1);

                mapPartBehavior partsToSink = temp.GetComponent<mapPartBehavior>();
                if (partsToSink != null)
                    partsToSink.stopSinking();
            }
            return true;
        }
        return false;
    }


    /// <summary>
    /// Draws the ring in coords; uses the brehenham's circle algorithm (aka midpoint circle algorithm)
    /// </summary>
    /// <param name="thickness">Thickness = how many iterations of the algorithm are required for the current map ring.</param>
    /// <param name="ring">Ring = game object to be passed along for the drawing calls.</param>
    public void ringDrawing(int thickness, GameObject ring)
    {
        int startRadius = radius;
        while (thickness + startRadius > radius)
        {
            int X = radius;
            int Y = 0;
            int Xchange = (1 - 2 * radius);
            int Ychange = 1;
            int radiusError = 0;
            while (X >= Y)
            {
                PutPixelsOnAllOctagonals(X * multiplier, Y * multiplier, ring);
                Y += 1;
                radiusError += Ychange;
                Ychange += 2;
                if (2 * radiusError + Xchange > 0)
                {
                    PutPixelsOnAllOctagonals(X * multiplier, Y * multiplier, ring);
                    X--;
                    radiusError += Xchange;
                    Xchange += 2;
                }
            }
            radius++;
        }
        mapData.BuildMesh();
    }

    /// <summary>
    /// Creates the individual "pixel" of the map at the necessary position and adds it the the current ring GameObject as a child.
    /// X and Z are the coordinates of the pixel on the map "grid".
    /// The new pixel is named after it's coordinates. 
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="z">The z coordinate.</param>
    /// <param name="ringNo">Ring no.</param>
    void PutPixel(int x, int z, GameObject ringNo)
    {
        mapData.addTile(x, 0, z, multiplier);
    }

    /// <summary>
    /// Puts the pixels on all octagonals - calls PutPixel for each of the 8 sign variation combinations in the x&z coords.
    /// Requires the ringNo game object to pass it along to the PutPixel calls. 
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="ringNo">Ring no.</param>
    void PutPixelsOnAllOctagonals(int x, int y, GameObject ringNo)
    {
        PutPixel(centerX + x, centerY + y, ringNo);
        PutPixel(centerX - x, centerY + y, ringNo);

        PutPixel(centerX - x, centerY - y, ringNo);
        PutPixel(centerX + x, centerY - y, ringNo);

        PutPixel(centerX + y, centerY + x, ringNo);
        PutPixel(centerX - y, centerY + x, ringNo);

        PutPixel(centerX - y, centerY - x, ringNo);
        PutPixel(centerX + y, centerY - x, ringNo);
    }

    public void somethingSunk() {
        if(isLocalPlayer) {
            layersSunk++;
            if (!isServer)
                CmdSomethingSunk(layersSunk);
        }
    }

    [Command]
    void CmdSomethingSunk(int sunkCount) {
        layersSunk = sunkCount;
    }
    
    public void SinkingSyncing(float time) {
        sinkInit = time;
        if(!isServer)
            CmdSinkingSyncing(time);
    }

    [Command]
    void CmdSinkingSyncing(float time) {
        sinkInit = time;
    }

    public void startSinking()
    {
        if (!startedSinking) {

            StartCoroutine(sink());
        }
        startedSinking = true;
    }

    /// <summary>
    /// Sink this instance = start sinking timer
    /// </summary>
    IEnumerator sink()
    {
        sinkingARing = false;
        print("init = " + sinkInit + ", sink-time = " + (sinkInit - (float)Network.time));
        yield return new WaitForSeconds((sinkInit - (float)Network.time) + timeToNextSink);
        if ((ringsSunk <= ringsSpawned) && (map.transform.childCount >= 2))
        {
            MapSunk();
            Debug.Log("sinking");
            sinkingARing = true;
        }
    }

    /// <summary>
    /// Maps the sunk.
    /// sink the necessary map rings, according to noPlayers
    /// </summary>
    void MapSunk()
    {
        SinkingSyncing((float)Network.time);
        //somethingSunk();
        int mapRingsNo = map.transform.childCount;
        Transform temp = map.transform.GetChild(mapRingsNo - 1);

        mapPartBehavior partsToSink = temp.GetComponent<mapPartBehavior>();
        partsToSink.MoveBelowWater();
        //Destroy (temp.gameObject,10f);
        ringsSunk++;

        //start timer for next sink
        StartCoroutine(sink());
    }
}
