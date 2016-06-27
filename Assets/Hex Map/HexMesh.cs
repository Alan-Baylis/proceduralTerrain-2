using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

	public string Name;
	Mesh mesh;
	List<Vector3> vertices;
	List<int> triangles;
	public MeshCollider meshCollider;

	public HexMesh(string name) {
		this.name = name;
	}

	//Setting components
	void Awake(){
		mesh = new Mesh ();
		GetComponent<MeshFilter> ().mesh = mesh;
		mesh.name = name;
		vertices = new List<Vector3>();
		triangles = new List<int> ();
		meshCollider = gameObject.AddComponent<MeshCollider>();
	}

	public void Triangulate (HexGridCell[] cells) {
		mesh.Clear();
		vertices.Clear();
		triangles.Clear();
		for (int i = 0; i < cells.Length; i++) {
			Triangulate(cells[i]);
			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			meshCollider.sharedMesh = mesh;
			for(int j = 0; i < 10; i++){
				print (j);
			}
		}


	}
	
	void Triangulate (HexGridCell cell) {
		for (int i = 0; i < 6; i++) {
			Vector3 center = cell.transform.localPosition;
			AddTriangle(
				center,
				center + MapConstants.hexCorners[i],
				center + MapConstants.hexCorners[(i+1) % 6]
				);
		}
	}

	void AddTriangle (Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}
}
