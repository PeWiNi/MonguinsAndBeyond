using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class mapCreator : MonoBehaviour {

	//public GameObject mapPart;
	public GameObject[] players;
	public int ringsSpawned=1;
	public int powCount;
	public GameObject map;
	float startTime;
	public float timeToNextSink;
	int ringsSunk;
	Vector3 center=new Vector3(0f,0f,0f);
	float RingRadius;
	bool sinkingARing;
	public int radius;
	public float thickness;

	public GameObject cube;
	//public GameObject players;
	public int centerX = 0;
	public int centerY = 0;
	public int multiplier;

	List <ringHandler> rings = new List<ringHandler> ();

	void Awake(){
		players=GameObject.FindGameObjectsWithTag("Player");
		ringsSpawned = 0;
		map=GameObject.FindGameObjectWithTag("Map");
		startTime = Time.realtimeSinceStartup;
		timeToNextSink = 15f;

		ringsSunk = 0;
		radius = 10;
		thickness = 8f;
		multiplier = 8;
		ringHandler temp = new ringHandler ();
		temp.ringParts.Add (GameObject.Find ("arena"));
		temp.ringNumber = ringsSpawned;
		rings.Add (temp);
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
		powCount--;
		//ringsSpawned= ((powCount>1) ? powCount: 1)-1;
		//radius = 5*((powCount>1) ? powCount: 1)+(int)(16f/ Mathf.Pow (2, powCount));
		//radius = radius+(int)(16f/ Mathf.Pow (2, powCount));
		thickness = 16f/ Mathf.Pow (2, powCount-1);

		ringsSpawned++;
		GameObject ringNo = new GameObject ();
		ringNo.transform.parent = map.transform;
		ringNo.transform.name = ringsSpawned.ToString ();
		ringDrawing ((int)thickness, ringNo); // need to deal with a float thickness to account for smaller rings. 


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
	{	int startRadius=radius;

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
		Vector3 position = new Vector3 ((float)x, 0f, (float)z);
		GameObject mapPartTemp=Instantiate (cube, position, Quaternion.identity) as GameObject;
		mapPartTemp.transform.parent = ringNo.transform;
		mapPartTemp.name = "mapPart_(" + x.ToString() + "," + z.ToString()+")_ring" + ringNo.ToString ();

		/*ringHandler temp = new ringHandler ();
		temp.ringParts.Add (mapPartTemp);
		temp.ringNumber = ringsSpawned;
		rings.Add (temp);*/

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

	/// <summary>
	/// Calculates the radius, starts from 0 = center and outwards to the ring for which the radius is needed. 
	/// </summary>
	/// <returns>The radius of the current ring.</returns>
	/// <param name="currentRing">Current ring.</param>
	/// <param name="endRing">End ring.</param>
	float calculateRadius(int currentRing, int endRing)
	{
		Debug.Log (currentRing + "currRing");
		if ((currentRing > 0) &&(currentRing < endRing))
	
				return (10 / (currentRing + 1)) + calculateRadius (currentRing--, endRing);
		else
			return 2f;
	}

	//start sinking timer
	IEnumerator sink(){
		sinkingARing = false;
		yield return new WaitForSeconds (timeToNextSink);
		if ((ringsSunk <= ringsSpawned) && (map.transform.childCount >= 17)) {
			MapSunk ();
			sinkingARing = true;
		}
	}

	/// <summary>
	/// Maps the sunk.
	/// sink the necessary map rings, according to noPlayers
	/// </summary>
	void MapSunk()
	{	
		//sink the outer ring
		int mapPartslength = map.transform.childCount;

		mapPartBehavior[] parts = map.GetComponentsInChildren<mapPartBehavior> ();
		for (int i =(mapPartslength - 17); i < mapPartslength-1; i++) {
			
			parts [i].MoveBelowWater ();

		}

		ringsSunk++;

		//start timer for next sink
		StartCoroutine (sink ());
	}
}
