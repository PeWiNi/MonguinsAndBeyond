using UnityEngine;
using System.Collections;

public class mapCreator : MonoBehaviour {

	public GameObject mapPart;
	public GameObject[] players;
	public int rings=1;
	public int powCount;
	public GameObject map;


	void Awake(){
		players=GameObject.FindGameObjectsWithTag("Player");
		rings = 1;
		map=GameObject.FindGameObjectWithTag("Map");

	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	}

	public void playerConnected()
	{
		players=GameObject.FindGameObjectsWithTag("Player");
		powCount=0;
		while(Mathf.Pow(2,powCount)<=players.Length)
		{
			powCount++;
		}

		// name=((city.getName()==null)? 'n'" city.getName())
		rings= ((powCount>1) ? powCount: 1);

		for (int i = (map.transform.childCount / 16) +1;i<=rings;i++)
		{ 
			float currentDegree=360f;
			while (currentDegree>0)
			{
				//change the rotation of the map part to create the ring
				Quaternion mapPartNewRotation=Quaternion.identity;
				mapPartNewRotation.eulerAngles=new Vector3(0,currentDegree,0);

				//assess its possition considering the number of rings existent
				Vector3 mapPartPosition=new Vector3(0f,0f,0f);
				if (rings > 1) {
					mapPartPosition.x=((currentDegree>90f && currentDegree<270f) ? -1f : 1f)*(float)(rings-1);
					mapPartPosition.z=((currentDegree>90f && currentDegree<270f) ? -1f : 1f)*(float)(rings-1);
				}
			

				GameObject mapPartTemp=Instantiate(mapPart, mapPartPosition, mapPartNewRotation) as GameObject;
				//set its parent as the Map GameObject
				mapPartTemp.transform.parent = map.transform;
				mapPartTemp.name = "mapPart_" + currentDegree.ToString () + "_ring" + (i).ToString ();

				//set scale according to which ring the part belongs to
				Vector3 newMapPartScale=new Vector3(1f,1f,1f);
				newMapPartScale.x *= rings;
				newMapPartScale.z *= rings;
				mapPartTemp.transform.localScale=newMapPartScale;

				currentDegree-=22.5f;
			
			}
		}



	}
}
