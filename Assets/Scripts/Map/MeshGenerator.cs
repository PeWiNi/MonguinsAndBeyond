using UnityEngine;
using System.Collections;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]

public class MeshGenerator : MonoBehaviour {

	Material material;
	Mesh mesh;
	Vector3 [] vertices;
	int[] triangles;
	Vector3 [] normals;

	Vector2 [] uv;

	public int size_x;
	public int size_z;

	int vsize_x;
	int vsize_z;
	int numVerts ;
	int numTiles ;
	int numTris;


	int vertIndex;
	int triIndex;
	// Update is called once per frame
	void Update () {
	
	}
		
	public void AlocateMeshData(int width, int height){
		
		size_x = width*4;
		size_z = height*4;
	
		numTiles = size_x * size_z;
		numTris = numTiles * 2;


		vsize_x = size_x + 1;
		vsize_z = size_z + 1;
		numVerts = vsize_x * vsize_z;
	
		// Allocate space for the mesh data
		vertices = new Vector3[numVerts];
		normals = new Vector3[numVerts];

		triangles = new int[ numTris * 3 ];
		vertIndex = 0;
		triIndex = 0;
		material = Resources.Load ("Materials/GroundZ") as Material;
	}


	public void addTile(float coordX, float elevation, float coordZ,  int tileSize)
	{ 
		vertices [vertIndex+0] = new Vector3 (coordX - tileSize / 2, elevation, coordZ - tileSize / 2);
		vertices [vertIndex+1] = new Vector3 (coordX - tileSize / 2, elevation, coordZ + tileSize / 2);
		vertices [vertIndex+2] = new Vector3 (coordX + tileSize / 2, elevation, coordZ - tileSize / 2);
		vertices [vertIndex+3] = new Vector3 (coordX + tileSize / 2, elevation, coordZ + tileSize / 2);

		for (int i = vertIndex; i < vertIndex + 4; i++)
			normals [i] = Vector3.up;

		triangles[triIndex + 0] = vertIndex+ 0;
		triangles[triIndex + 1] = vertIndex+ 3;
		triangles[triIndex + 2] = vertIndex+ 2;

		triangles [triIndex + 3] = vertIndex + 0;
		triangles [triIndex + 4] = vertIndex + 1;
		triangles [triIndex + 5] = vertIndex + 3;

		vertIndex += 4;
		triIndex += 6;
	}

	public void BuildMesh() {
			
		// Create a new Mesh and populate with the data
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uv;
		mesh.name = gameObject.transform.name;
		
		// Assign our mesh to our filter/renderer/collider
		MeshFilter mesh_filter = GetComponent<MeshFilter>();
		MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
		MeshCollider mesh_collider = GetComponent<MeshCollider>();
		
		mesh_filter.mesh = mesh;
		mesh_collider.sharedMesh = mesh;
		mesh_renderer.material = material;

	}
}
