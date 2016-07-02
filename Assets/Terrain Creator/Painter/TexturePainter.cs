/// <summary>
/// CodeArtist.mx 2015
/// This is the main class of the project, its in charge of raycasting to a model and place brush prefabs infront of the canvas camera.
/// If you are interested in saving the painted texture you can use the method at the end and should save it to a file.
/// </summary>


using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum Painter_BrushMode{PAINT,DECAL};
public class TexturePainter : MonoBehaviour {
	
	public GameObject terrainObject;
	private int visualizationMode = 0; //0 Visualization; 1 Edit Humidity; 2 Edit Height; 3 Edit Temp

	//Humidity
	public GameObject brushCursor,brushContainer; 
	public Camera sceneCamera,canvasCam;  
	public Sprite cursorPaint;  
	public RenderTexture canvasTexture;
	public RenderTexture saveTexture;
	public Material defaultTerrainMaterial;
	public Material humidtyPainterMaterial;
	public Material humidityCanvasMaterial;

	//Height
	public GameObject hbrushCursor,hbrushContainer; 
	public Camera hcanvasCam;  
	public Sprite hcursorPaint;  
	public RenderTexture hcanvasTexture;
	public RenderTexture hsaveTexture;
	public Material heightPainterMaterial;
	public Material heightCanvasMaterial;

	//Temperature
	public GameObject tbrushCursor,tbrushContainer; 
	public Camera tcanvasCam;  
	public Sprite tcursorPaint;  
	public RenderTexture tcanvasTexture;
	public RenderTexture tsaveTexture;
	public Material temperaturePainterMaterial;
	public Material temperatureCanvasMaterial;

	//CurrentModePainter
	private Camera currentCanvasCam;
	private RenderTexture currentRT;
	private RenderTexture currentSaveRT;
	private Material currentMaterial;
	private Material currentCanvasMaterial;
	private GameObject currentBrushCursor, currentBrushContainer;
	public Slider sizeSlider;
	public Slider colorSlider;

	Painter_BrushMode mode; //Our painter mode (Paint brushes or decals)
	float brushSize=1.0f; //The size of our brush
	Color brushColor = new Color(0.5f,0.5f,0.5f); //The selected color
	int brushCounter=0,MAX_BRUSH_COUNT=100; //To avoid having millions of brushes
	bool saving=false; //Flag to check if we are saving the texture

	public void setVisualization(int mode){
	    this.visualizationMode = mode;
		if (mode == 0) {
			terrainObject.GetComponent<MeshRenderer> ().material = defaultTerrainMaterial;
			generateBiomes ();
		} else if (mode == 1) {
			print ("Entering Humidity Painter Mode");
			terrainObject.GetComponent<MeshRenderer> ().material = humidtyPainterMaterial;
			this.currentCanvasCam = canvasCam;
			this.currentRT = canvasTexture;
			this.currentSaveRT = saveTexture;
			this.currentMaterial = humidtyPainterMaterial;
			this.currentCanvasMaterial = humidityCanvasMaterial;
			this.currentBrushCursor = brushCursor;
			this.currentBrushContainer = brushContainer;

		} else if (mode == 2) {
			print ("Entering Height Painter Mode");
			terrainObject.GetComponent<MeshRenderer> ().material = heightPainterMaterial;
			this.currentCanvasCam = hcanvasCam;
			this.currentRT = hcanvasTexture;
			this.currentSaveRT = hsaveTexture;
			this.currentMaterial = heightPainterMaterial;
			this.currentCanvasMaterial = heightCanvasMaterial;
			this.currentBrushCursor = hbrushCursor;
			this.currentBrushContainer = hbrushContainer;

		} else if (mode == 3) {
			print ("Entering Temperature Painter Mode");
			terrainObject.GetComponent<MeshRenderer> ().material = temperaturePainterMaterial;
			this.currentCanvasCam = tcanvasCam;
			this.currentRT = tcanvasTexture;
			this.currentSaveRT = tsaveTexture;
			this.currentMaterial = temperaturePainterMaterial;
			this.currentCanvasMaterial = temperatureCanvasMaterial;
			this.currentBrushCursor = tbrushCursor;
			this.currentBrushContainer = tbrushContainer;
		}

		terrainObject.GetComponentInChildren<TerrainCreator>().activeRT = currentRT; 
		terrainObject.GetComponentInChildren<TerrainCreator>().mode = visualizationMode;
	}

	void Update () {
	    if (visualizationMode != 0) {
	      //brushColor = ColorSelector.GetColor (); //Updates our painted color with the selected color
	      if (Input.GetMouseButton(0)) {
	        DoAction();
	      }
	      UpdateBrushCursor ();
	    }
	}

	//The main action, instantiates a brush or decal entity at the clicked position on the UV map
	void DoAction(){  
		if (saving)
			return;
		Vector3 uvWorldPosition=Vector3.zero;   
		if(HitTestUVPosition(ref uvWorldPosition)){
			GameObject brushObj;
			brushObj=(GameObject)Instantiate(Resources.Load("Instances/BrushEntity")); //Paint a brush
			brushObj.GetComponent<SpriteRenderer>().color=brushColor; //Set the brush color
			brushColor.a=brushSize*2.0f; // Brushes have alpha to have a merging effect when painted over.
			brushObj.transform.parent= this.currentBrushContainer.transform; //Add the brush to our container to be wiped later
			brushObj.transform.localPosition=uvWorldPosition; //The position of the brush (in the UVMap)
			brushObj.transform.localScale=Vector3.one*brushSize;//The size of the brush
			brushObj.layer = 5;
			if (visualizationMode == 2) {
				Invoke ("updateHeight", 0.1f);
			}
		}
		brushCounter++; //Add to the max brushes
		if (brushCounter >= MAX_BRUSH_COUNT) { //If we reach the max brushes available, flatten the texture and clear the brushes
			//this.currentBrushCursor.SetActive (false);
			saving=true;
			Invoke("SaveTexture",0.1f);
		}
	}
	//To update at realtime the painting cursor on the mesh
	void UpdateBrushCursor(){
		Vector3 uvWorldPosition=Vector3.zero;

		if (HitTestUVPosition (ref uvWorldPosition)) {
			Cursor.visible = false;
		} else {
			Cursor.visible = true;
		} 

		if (HitTestUVPosition (ref uvWorldPosition) /**&& !saving*/) {
			this.currentBrushCursor.SetActive(true);
			this.currentBrushCursor.transform.position =uvWorldPosition + this.currentBrushContainer.transform.position;                  
		} else {
			//this.currentBrushCursor.SetActive(false);
		}
			
	}
	//Returns the position on the texuremap according to a hit in the mesh collider
	bool HitTestUVPosition(ref Vector3 uvWorldPosition){
		RaycastHit hit;
		Vector3 cursorPos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0.0f);
		Ray cursorRay=sceneCamera.ScreenPointToRay (cursorPos);
		if (Physics.Raycast(cursorRay,out hit,200)){
			MeshCollider meshCollider = hit.collider as MeshCollider;
			if (meshCollider == null || meshCollider.sharedMesh == null)
				return false;     
			Vector2 pixelUV  = new Vector2(hit.textureCoord.x,hit.textureCoord.y);
			uvWorldPosition.x=pixelUV.x - this.currentCanvasCam.orthographicSize;//To center the UV on X
			uvWorldPosition.y=pixelUV.y - this.currentCanvasCam.orthographicSize;//To center the UV on Y
			uvWorldPosition.z=0.0f;
			return true;
		}
		else{   
			return false;
		}

	}

	void SaveTexture(){   
		brushCounter=0;
		System.DateTime date = System.DateTime.Now;
		RenderTexture.active = this.currentSaveRT;
		Texture2D tex = new Texture2D(this.currentSaveRT.width, this.currentSaveRT.height, TextureFormat.RGB24, false);   
		tex.ReadPixels (new Rect (0, 0, this.currentSaveRT.width, this.currentSaveRT.height), 0, 0);
		tex.Apply ();
		this.currentCanvasMaterial.mainTexture = tex;

		foreach (Transform child in this.currentBrushContainer.transform) {//Clear brushes
			Destroy(child.gameObject);
		}
//		StartCoroutine (SaveTextureToFile(tex));
		Invoke ("ShowCursor", 0.1f);
		Invoke("generateBiomes", 0.1f);
	}

	void updateHeight(){
		if (visualizationMode == 2) {
			terrainObject.GetComponentInChildren<TerrainCreator> ().updateHeight();
		}
	}

	void generateBiomes(){
		if (visualizationMode == 1 || visualizationMode == 3) {
			terrainObject.GetComponentInChildren<TerrainCreator> ().updateGrayscales();
		}
	}

	//Show again the user cursor (To avoid saving it to the texture)
	void ShowCursor(){  
		saving = false;
	}

	public void SetBrushSize(float newBrushSize){ //Sets the size of the cursor brush or decal
		brushSize = newBrushSize;
		this.currentBrushCursor.transform.localScale = Vector3.one * brushSize;
	}

	public void SetBrushColor(float newBrushIntensity){ //Sets the size of the cursor brush or decal
		TerrainCreator script = terrainObject.GetComponentInChildren<TerrainCreator> ();
		if (visualizationMode == 1) {
			brushColor = script.humidityColoring.Evaluate(newBrushIntensity);
			//brushColor = new Color(newBrushIntensity, newBrushIntensity, newBrushIntensity);
		} else if (visualizationMode == 3) {
			brushColor = script.temperatureColoring.Evaluate( 1- newBrushIntensity);
			//brushColor = new Color(newBrushIntensity, newBrushIntensity, newBrushIntensity);
		} else {
			brushColor = new Color(newBrushIntensity, newBrushIntensity, newBrushIntensity);
		}

	}

	public void SaveTextureButtonPressed(){ //Saves Texture to assets folder
		brushCounter=0;
		System.DateTime date = System.DateTime.Now;
		RenderTexture.active = this.currentRT;
		Texture2D tex = new Texture2D(this.currentRT.width, this.currentRT.height, TextureFormat.RGB24, false);   
		tex.ReadPixels (new Rect (0, 0, this.currentRT.width, this.currentRT.height), 0, 0);
		tex.Apply ();
		this.currentCanvasMaterial.mainTexture = tex;

		foreach (Transform child in this.currentBrushContainer.transform) {//Clear brushes
			Destroy(child.gameObject);
		}
		StartCoroutine (SaveTextureToFile(tex));
		Invoke ("ShowCursor", 0.1f);
	}

	public void randomizeHumidityAndTemperature(){
		terrainObject.GetComponentInChildren<TerrainCreator>().randomHumidityTemperature();
	}

	IEnumerator SaveTextureToFile(Texture2D savedTexture){    
		brushCounter=0;
		string fullPath=System.IO.Directory.GetCurrentDirectory()+"/Assets/Terrain Creator/Textures/";
		System.DateTime date = System.DateTime.Now;
		string fileName = modeToString(visualizationMode)+"Texture.png";
		if (!System.IO.Directory.Exists(fullPath))    
			System.IO.Directory.CreateDirectory(fullPath);
		var bytes = savedTexture.EncodeToPNG();
		System.IO.File.WriteAllBytes(fullPath+fileName, bytes);
		Debug.Log ("<color=orange>" +modeToString(visualizationMode) + " Texture Saved</color>"+fullPath+fileName);
		yield return null;
	}

	public void clearCanvasBrushes(){
		foreach (Transform child in this.brushContainer.transform) {
			Destroy(child.gameObject);
		}
		foreach (Transform child in this.hbrushContainer.transform) {
			Destroy(child.gameObject);
		}
		foreach (Transform child in this.tbrushContainer.transform) {
			Destroy(child.gameObject);
		}
	}

	public void updateCursor(){
		
	}

	public string modeToString(int mode){
		switch (mode) {
		case 0:
			return "Default";
		case 1:
			return "Humidity";
		case 2:
			return "Height";
		case 3:
			return "Temperature";

		default:
			return "Undefined";
		}
	}
}
