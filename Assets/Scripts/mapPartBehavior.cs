using UnityEngine;
using System.Collections;

public class mapPartBehavior : MonoBehaviour {

	public bool sinking=false;
	Vector3 startPos;
	Vector3 endPos;
	float startTime;
	float speed =0.75f;
	float journeyLength;
	bool shouldSink=false;
	// Use this for initialization
	void Start () {
		startPos = transform.position;
		endPos=new Vector3(startPos.x, -10f, startPos.z);
		journeyLength = Mathf.Abs (endPos.y - startPos.y);
	}
	
	// Update is called once per frame
	void Update () {

		if (sinking) {
			
		float distCovered = (Time.time - startTime) * speed;
        float fracJourney = distCovered / journeyLength;
			transform.position = Vector3.Lerp(startPos, endPos, fracJourney);
		
		}
	}

	public void MoveBelowWater()
	{	
		sinking = true;
		startTime = Time.time;			
		Color c = gameObject.GetComponent<MeshRenderer>().material.color;
		gameObject.GetComponent<MeshRenderer>().material.color = new Color(c.r, c.g, c.b, 0.25f);
		StartCoroutine(DestroyAfterSink ());
	}
	IEnumerator DestroyAfterSink()
	{
		yield return new WaitForSeconds (10f);
		if (sinking) Destroy (gameObject);
	}

	public void stopSinking()
	{
		sinking = false;
		Color c = gameObject.GetComponent<MeshRenderer>().material.color;
		gameObject.GetComponent<MeshRenderer>().material.color = new Color(c.r, c.g, c.b, 1f);
		transform.position = startPos;
	}
}
