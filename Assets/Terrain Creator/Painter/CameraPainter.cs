using UnityEngine;
using System.Collections;

public class CameraPainter : MonoBehaviour {

	public Camera camera;
	public RenderTexture canvas;
	private int brushCounter = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void MergeTexture(){
		RenderTexture.active = canvas;
		int width = canvas.width;
		int height = canvas.height;
		Texture2D text = new Texture2D (width, height, TextureFormat.ARGB32, false);
		text.ReadPixels (new Rect (0, 0, width, height), 0, 0);
		text.Apply();
		RenderTexture.active = null;
		GetComponent<MeshRenderer> ().material.mainTexture = text;

		//foreach(Transform brush in brushContainer.transform){
		//	Destroy (brush.gameObject);
		//}

		brushCounter = 0;
	}

	bool HitTestUVPosition(ref Vector3 uvWorldPosition) {
		RaycastHit hit;
		Vector3 mousePos = Input.mousePosition;
		Vector3 cursorPos = new Vector3 (mousePos.x, mousePos.y, 0.0f);

		Ray cursorRay = camera.ScreenPointToRay (cursorPos);
		if (Physics.Raycast (cursorRay, out hit, 200)) {
			MeshCollider collider = hit.collider as MeshCollider;

			if (collider == null || collider.sharedMesh == null){
				return false;
			}

			Vector2 hitUV = new Vector2 (hit.textureCoord.x, hit.textureCoord.y);
			uvWorldPosition.x = hitUV.x;
			uvWorldPosition.y = hitUV.y;
			uvWorldPosition.z = 0.0f;
			return true;
		}

		return false;
	}
}
