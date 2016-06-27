using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

	public int sections = 4;
	public int width = 100;
	public int height = 100;
	public HexGridCell cellPrefab;
	public Text cellLabelPrefab;

	HexGridCell[] gridCells;
	Canvas gridCanvas;
	HexMesh hexMesh;

	void Awake () {

		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh> ();
		gridCells = new HexGridCell[height * width];
		int i = 0;

		for (int h = 0; h < height; h++) {
			for (int w = 0; w < width; w++) {
				CreateHex(h, w, i);
				i++;
			}
		}
	}

	void Start () {
		hexMesh.Triangulate(gridCells);
		hexMesh.meshCollider.convex = true;
		hexMesh.meshCollider.convex = false;
	}

	void CreateHex (int x, int z, int i) {
		Vector3 position;
		position.x = (x + z * 0.5f -z /2) * (MapConstants.hexInnerRadius * 2f);
		position.y = 0f;
		position.z = z * (MapConstants.hexOuterRadius * 1.5f);
		
		HexGridCell cell = gridCells[i] = (HexGridCell)GameObject.Instantiate(cellPrefab);
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

		Text label = (Text)GameObject.Instantiate(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
	}

	void Update () {
		if (Input.GetMouseButton(0)) {
			HandleInput();
		}
	}
	
	void HandleInput () {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			TouchCell(hit.point);
		}
	}
	
	void TouchCell (Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		Debug.Log("touched at " + coordinates.ToString());
	}
}
