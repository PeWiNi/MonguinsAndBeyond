using UnityEngine;
using System.Collections;

public class mapCreator : MonoBehaviour {

	public GameObject mapPart;
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

	void Awake(){
		players=GameObject.FindGameObjectsWithTag("Player");
		ringsSpawned = 1;
		map=GameObject.FindGameObjectWithTag("Map");
		startTime = Time.realtimeSinceStartup;
		timeToNextSink = 15f;

		ringsSunk = 0;

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
		while(Mathf.Pow(2,powCount)<=players.Length)
		{
			powCount++;
		}
		ringsSpawned= ((powCount>1) ? powCount: 1)-1;

		if (!sinkingARing)
			for (int i = (map.transform.childCount / 16); i < ringsSpawned - ringsSunk; i++)
			{ 
				spawnMapRing (i);
			}
		else {
				mapPartBehavior[] parts = map.GetComponentsInChildren<mapPartBehavior> ();
				for (int i = parts.Length - 17; i < parts.Length; i++) {
					parts [i].stopSinking ();
				}
			}
	}

	void spawnMapRing(int i)
	{
		//radius needed for snowflakes calculations 
		//RingRadius = calculateRadius (0,i);

		float currentDegree=360f;

		while (currentDegree>0)
		{
			//change the rotation of the map part to create the ring
			Quaternion mapPartNewRotation=Quaternion.identity;
			mapPartNewRotation.eulerAngles=new Vector3(0,currentDegree,0);


			Vector3 mapPartPosition=new Vector3(0f,0f,0f);

			//the calculation below were meant to be used in the map generation that takes into account the necessary increase shrink 
			//of rings' radius as the map rings increase in number. 
			// they make snowflakes instead

			//mapPartPosition.x = center.x + RingRadius * Mathf.Cos (currentDegree);
			//mapPartPosition.z = center.z + RingRadius * Mathf.Sin (currentDegree);
			//or
			//mapPartPosition.x = ((currentDegree > 90f && currentDegree <= 270f) ? -1.25f : 1.25f)* (float)(i);
			//mapPartPosition.z = ((currentDegree > 0f && currentDegree <= 180f) ? -1.25f : 1.25f)* (float)(i)*Mathf.Pow(2,i);

			GameObject mapPartTemp=Instantiate(mapPart, mapPartPosition, mapPartNewRotation) as GameObject;

			//set its parent as the Map GameObject
			mapPartTemp.transform.parent = map.transform;
			mapPartTemp.name = "mapPart_" + currentDegree.ToString () + "_ring" + (i+1).ToString ();

			//set scale according to which ring the part belongs to
			Vector3 newMapPartScale=new Vector3(1f,1f,1f);
			newMapPartScale.x *= Mathf.Pow(2,i);

			newMapPartScale.z *= Mathf.Pow(2,i);
			mapPartTemp.transform.localScale=newMapPartScale;

			currentDegree-=22.5f;

		}
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
