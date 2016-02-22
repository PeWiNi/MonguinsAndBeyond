using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ringHandler {

	public int ringSize;
	public Vector3[,] ringParts; 
	public int[,] ringPartsint;
	public int ringNumber;

	// Use this for initialization

	public void createVectorArray(int size){
		ringParts=new Vector3[size,size];
		ringPartsint = new int[size, size];
	}

	// Update is called once per frame
	void Update () {
	
	}
}
