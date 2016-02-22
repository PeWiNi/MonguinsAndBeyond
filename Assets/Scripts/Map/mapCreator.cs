using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class mapCreator : MonoBehaviour {

	//public GameObject mapPart;
	public GameObject[] players;
	public int ringsSpawned=1;
	public int powCount;
	public GameObject map;
	//float startTime;
	public float timeToNextSink;
	int ringsSunk;
	Vector3 center=new Vector3(0f,0f,0f);
	float RingRadius;
	bool sinkingARing;
	public int radius;
	public float thickness;

	public GameObject cube;
//	public ringHandler[] rings;
	public int[,] ringPartsint=new int[1000,1000];
	int centerOffset;

	public int centerX = 0;
	public int centerY = 0;
	public int multiplier; //correlates with the cube size 


	void Awake(){
		players=GameObject.FindGameObjectsWithTag("Player");
		ringsSpawned = 0;
	//	rings = new ringHandler[8];
		map=GameObject.FindGameObjectWithTag("Map");
	//	startTime = Time.realtimeSinceStartup;
		timeToNextSink = 15f;

		ringsSunk = 0;
		radius = 10;
		thickness = 8f;
		multiplier = 8;

	}

	// Use this for initialization
	void Start () {
		StartCoroutine (sink ());
	}
	
	// Update is called once per frame
	void Update () {
	}

	/// <summary>
	/// Modifies the map according to the number players connected.
	/// </summary>
	public void playerConnected()
	{
		players=GameObject.FindGameObjectsWithTag("Player");
		powCount=0;
		while(Mathf.Pow (2, powCount) <= players.Length) 
			powCount++;
		
		powCount--;//need to go one step back as the loop above will stop when one step further in the calculation of the pow count 

		// if the number of players requires more map rings than already present, generate them.
		if (powCount>map.transform.childCount-1) 
		{
			//thickness represents how many extra ring components will the new map ring contain, based on the number of players
			thickness = 8f/ Mathf.Pow (2, powCount-1);

			ringsSpawned++; //increase the number of rings Spawned
			GameObject ringNo = new GameObject (); //create a new Ring parent for the the map parts to be spawned
			ringNo.transform.parent = map.transform;
			ringNo.transform.name = ringsSpawned.ToString ();
			ringNo.transform.tag = "Ring";
			centerOffset=(int)(thickness+radius)*multiplier;
			Debug.Log("centerOffset:" +centerOffset); 
			//rings[ringsSpawned].createVectorArray(centerOffset*2);

			ringDrawing ((int)thickness, ringNo); // need to deal with a float thickness to account for smaller rings. 
		}

	//	fillCheck ();

		/*if (!sinkingARing)
			for (int i = (map.transform.childCount / 16); i < ringsSpawned - ringsSunk; i++)
			{ 
				spawnMapRing (i);
			}
		else {
				mapPartBehavior[] parts = map.GetComponentsInChildren<mapPartBehavior> ();
				for (int i = parts.Length - 17; i < parts.Length; i++) {
					parts [i].stopSinking ();
				}
			}*/
	}



	public void ringDrawing(int thickness, GameObject ring)
	{	
		int startRadius=radius;
		while (thickness+startRadius >radius) {
			int	X = radius;
			int	Y = 0;
			int Xchange = (1 - 2 * radius);
			int Ychange = 1;
			int radiusError = 0;
			while (X >= Y) {
				PutPixelsOnAllOctagonals (X * multiplier, Y * multiplier, ring);
				Y += 1;
				radiusError += Ychange;
				Ychange += 2;
				if (2 * radiusError + Xchange > 0) {
					PutPixelsOnAllOctagonals (X * multiplier, Y * multiplier, ring);
					X--;
					radiusError += Xchange;
					Xchange += 2;
				}
			}
			radius++;
		}
	}

	void PutPixel(int x, int z, GameObject ringNo)
	{
		//Debug.Log (x + "," + z);
		if (ringPartsint [x + centerOffset, z + centerOffset] < ringsSpawned*2) {
			ringPartsint [x + centerOffset, z + centerOffset] += ringsSpawned;
			Vector3 position = new Vector3 ((float)x, 0f, (float)z);
			GameObject mapPartTemp = Instantiate (cube, position, Quaternion.identity) as GameObject;
			mapPartTemp.transform.parent = ringNo.transform;
			mapPartTemp.name = "mapPart_(" + x.ToString () + "," + z.ToString () + ")";

			//rings[ringsSpawned].ringParts[x+centerOffset, z+centerOffset]=new Vector3(x+centerOffset, z+centerOffset,ringsSpawned);

		}
	}


	void PutPixelsOnAllOctagonals(int x,int y, GameObject ringNo){

		PutPixel (centerX + x, centerY + y, ringNo);
		PutPixel (centerX - x, centerY + y, ringNo);

		PutPixel (centerX - x, centerY - y, ringNo);
		PutPixel (centerX + x, centerY - y, ringNo);

		PutPixel (centerX + y, centerY + x, ringNo);
		PutPixel (centerX - y, centerY + x, ringNo);

		PutPixel (centerX - y, centerY - x, ringNo);
		PutPixel (centerX + y, centerY - x, ringNo);

	}
		
	//start sinking timer
	IEnumerator sink(){
		sinkingARing = false;
		yield return new WaitForSeconds (timeToNextSink);
		if ((ringsSunk <= ringsSpawned) && (map.transform.childCount >=2)) {
			MapSunk ();
			Debug.Log ("sinking");
			sinkingARing = true;
		}
	}

	/// <summary>
	/// Maps the sunk.
	/// sink the necessary map rings, according to noPlayers
	/// </summary>
	void MapSunk()
	{	
		int mapRingsNo= map.transform.childCount;
		Transform temp = map.transform.GetChild (mapRingsNo - 1);
	
		mapPartBehavior[] partsToSink =temp.GetComponentsInChildren<mapPartBehavior>();
		for (int i =0; i < partsToSink.Length; i++) {
			partsToSink [i].MoveBelowWater ();
		}
		Destroy (temp.gameObject,10f);
		ringsSunk++;

		//start timer for next sink
		StartCoroutine (sink ());
	}

	void fillCheck()
	{
		for (int i = 8; i < radius * 8; i = i + 8) {
			for (int j = 8; j < radius * 8; j = j + 8)
				if (!GameObject.Find ("mapPart_(" + i.ToString () + "," + j.ToString () + ")"))
					Debug.Log ("found empty spot at: " + i + " ," + j);	
		}
	}
}
