using UnityEngine;
using System.Collections;

public class circleGenerator : MonoBehaviour {
	public int x;
	public int z;
	public int radius;
	public GameObject cube;
	public GameObject cubePlayer;
	public int centerX = 0;
	public int centerY = 0;
	public int X;
	public int Y;
	public int Xchange;
	public int Ychange;
	public int radiusError;

	// Use this for initialization
	void Start () {

		x =0;
		z = 0;
		//radius = 16;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void circleDrawing()
	{

	//The center of the circle and its radius.
	//int x = 100;
	//int z = 100;
	//int r = 50;
	//This here is sin(45) but i just hard-coded it.
		float sinus = Mathf.Sin(45);
	//This is the distance on the axis from sin(90) to sin(45). 

		int range = (int)(radius/(2*sinus));
	for(int i = radius-1 ; i >= range ; --i)
	{
		int j = (int)Mathf.Sqrt(radius*radius - i*i);
		for(int k = -j ; k <= j ; k++)
		{
			//We draw all the 4 sides at the same time.
			PutPixel(x-k,z+i);
			PutPixel(x-k,z-i);
			PutPixel(x+i,z+k);
			PutPixel(x-i,z-k);
		}
	}
	//To fill the circle we draw the circumscribed square.
	/*	range = (int)( radius*sinus);
	for(int i = x - range + 1 ; i < x + range ; i++)
	{
		for(int j = z- range + 1 ; j <z + range ; j++)
		{
			PutPixel(i,j);
		}
		
		}*/
	}
	
	void PutPixel(int x, int z)
	{
		Vector3 position = new Vector3 ((float)x, 0f, (float)z);
		//	GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		Instantiate (cube, position, Quaternion.identity);
	}


	//bresenham algorithm for drawing circles
	public void circleDrawing2()
	{	
		Instantiate (cubePlayer, transform.position, Quaternion.identity);
		radius = GameObject.FindGameObjectsWithTag ("Player").Length;
		X = radius;
		Y = 0;
		Xchange = 1 - 2 * radius;
		Ychange = 1;
		radiusError = 0;
		while (X >= Y) {
			PutPixels2 (X, Y);
			Y++;
			radiusError += Ychange;
			Ychange += 2;
			if (2 * radiusError + Xchange > 0) {
				X--;
				radiusError += Xchange;
				Xchange += 2;
			}
		}
	}

	void PutPixels2(int x,int y){
			PutPixel (centerX + x, centerY + y);
			PutPixel (centerX - x, centerY + y);

			PutPixel (centerX - x, centerY - y);
			PutPixel (centerX + x, centerY - y);

			PutPixel (centerX + y, centerY + x);
			PutPixel (centerX - y, centerY + x);

			PutPixel (centerX - y, centerY - x);
			PutPixel (centerX + y, centerY - x);


	}
}
