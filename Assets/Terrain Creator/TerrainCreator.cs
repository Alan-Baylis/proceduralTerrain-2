using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TerrainCreator : MonoBehaviour {

	public enum gridType{
		Quad,
		Hexagon
	};

	//Terrain logic and rendering related variables
	private Mesh mesh;
	private GameObject waterPlane;
	private int currentResolution;
	private float currentSize;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Color[] colors;
	Vector3 randomOffset;
	float randomHuValue;
	float randomTempValue;
	private Texture2D huTex;
	private Texture2D huGrayTex;
	private Texture2D tempTex;
	private Texture2D tempGrayTex;
	private Texture2D tex;

	public GameObject grayscaler;
	public GameObject painterScript;
	public GameObject hPainter;
	public GameObject huPainter;
	public GameObject tPainter;
	public Vector3 offset;
	public Vector3 rotation;
	public bool strenghtModifiesColors = true;
	public Material waterMaterial;
	public Shader waterShader;
	public int randomSeed = 0;
	public bool fixSeed = false;

	//Biomes
	//1 - polar, 2 - tundra, 3 - boreal, 4 - cold desert
	//5 - praire, 6 - temp. forest, 7 -warm desert
	//8 - grassland, 9 - savana, 10 - trop forest, 11- trop rain forest
	public GameObject biomes;
	public Camera BiomesCamera;
	public GameObject huGray;
	public GameObject tempGray;
	private Texture2D polarText;
	private Texture2D tundraText;
	private Texture2D borealText;
	private Texture2D coldDesertText;
	private Texture2D praireText;
	private Texture2D tempForestText;
	private Texture2D warmDesertText;
	private Texture2D grasslandText;
	private Texture2D savanaText;
	private Texture2D tropForestText;
	private Texture2D rainForestText;

	public RenderTexture activeRT;
	public int mode =0;
	public float textureColorExposure = 2f;

	//Terrain Parameteres:
	[Range(5, 254)]
	public int resolution = 10;
	[Range(1, 10)]
	public int chunks = 1;
	public float chunkSize;

	//Noise Parameters
	public float frequency = 1f;
	public NoiseMethodType type;
	public Gradient coloring;
	public Gradient humidityColoring;
	public Gradient temperatureColoring;
	public Gradient heightColoring;

	[Range(0f, 1f)]
	public float strength = 1f;
	[Range(1, 15)]
	public int octaves = 1;
	[Range(1f, 4f)]
	public float lacunarity = 2f;
	[Range(0f, 1f)]
	public float persistence = 0.5f;
	[Range(1, 3)]
	public int dimensions = 3;


	//Create the empty mesh
	private void OnEnable () {
		if (mesh == null) {
			mesh = new Mesh();
			mesh.name = "My Terrain";
			GetComponentInParent<MeshFilter>().mesh = mesh;

			//Creating the water plane based on maps dimensions
			waterPlane = GameObject.CreatePrimitive (PrimitiveType.Plane);
			waterPlane.name = "Water Plane";
			MeshRenderer waterRenderer = waterPlane.GetComponent<MeshRenderer>();
			waterRenderer.material = waterMaterial;
			waterRenderer.material.shader = waterShader;
			waterPlane.AddComponent<WaterSimple>();
			//waterPlane.GetComponent<MeshCollider> ().enabled = false;

			//Creating default Textures
			huTex = new Texture2D (resolution, resolution, TextureFormat.ARGB32, true);
			huGrayTex = new Texture2D (resolution, resolution, TextureFormat.ARGB32, true);
			tempTex = new Texture2D (resolution, resolution, TextureFormat.ARGB32, true);
			tempGrayTex = new Texture2D (resolution, resolution, TextureFormat.ARGB32, true);
			tex = new Texture2D (resolution, resolution, TextureFormat.ARGB32, true);
			tex.name = "Heightmap";
			huTex.name = "HumidityMap";
			huGrayTex.name = "HumidityGrayscaleMap";
			tempTex.name = "TemperatureText";
			tempGrayTex.name = "TemperatureGrayscaleText";
			tex.wrapMode = TextureWrapMode.Clamp;
			huTex.wrapMode = TextureWrapMode.Clamp;
			huGrayTex.wrapMode = TextureWrapMode.Clamp;
			tempTex.wrapMode = TextureWrapMode.Clamp;
			tempGrayTex.wrapMode = TextureWrapMode.Clamp;
			tex.filterMode = FilterMode.Trilinear;
			huTex.filterMode = FilterMode.Trilinear;
			huGrayTex.filterMode = FilterMode.Trilinear;
			tempTex.filterMode = FilterMode.Trilinear;
			tempGrayTex.filterMode = FilterMode.Trilinear;
			tex.anisoLevel = 0;
			huTex.anisoLevel = 0;
			huGrayTex.anisoLevel = 0;
			tempTex.anisoLevel = 0;
			tempGrayTex.anisoLevel = 0;
			hPainter.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTexture = tex;
			huPainter.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTexture = huTex;
			tPainter.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTexture = tempTex;
			huGray.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = huGrayTex;
			tempGray.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = tempGrayTex;

			polarText = biomes.GetComponentsInChildren<MeshRenderer> () [0].sharedMaterial.mainTexture as Texture2D;
			polarText.wrapMode = TextureWrapMode.Clamp;
			tundraText = biomes.GetComponentsInChildren<MeshRenderer> () [1].sharedMaterial.mainTexture as Texture2D;
			tundraText.wrapMode = TextureWrapMode.Clamp;
			borealText = biomes.GetComponentsInChildren<MeshRenderer> () [2].sharedMaterial.mainTexture as Texture2D;
			borealText.wrapMode = TextureWrapMode.Clamp;
			coldDesertText = biomes.GetComponentsInChildren<MeshRenderer> () [3].sharedMaterial.mainTexture as Texture2D;
			coldDesertText.wrapMode = TextureWrapMode.Clamp;
			praireText  = biomes.GetComponentsInChildren<MeshRenderer> () [4].sharedMaterial.mainTexture as Texture2D;
			praireText.wrapMode = TextureWrapMode.Clamp;
			tempForestText   = biomes.GetComponentsInChildren<MeshRenderer> () [5].sharedMaterial.mainTexture as Texture2D;
			tempForestText.wrapMode = TextureWrapMode.Clamp;
			warmDesertText   = biomes.GetComponentsInChildren<MeshRenderer> () [6].sharedMaterial.mainTexture as Texture2D;
			warmDesertText.wrapMode = TextureWrapMode.Clamp;
			grasslandText   = biomes.GetComponentsInChildren<MeshRenderer> () [7].sharedMaterial.mainTexture as Texture2D;
			grasslandText.wrapMode = TextureWrapMode.Clamp;
			savanaText   = biomes.GetComponentsInChildren<MeshRenderer> () [8].sharedMaterial.mainTexture as Texture2D;
			savanaText.wrapMode = TextureWrapMode.Clamp;
			tropForestText   = biomes.GetComponentsInChildren<MeshRenderer> () [9].sharedMaterial.mainTexture as Texture2D;
			tropForestText.wrapMode = TextureWrapMode.Clamp;
			rainForestText   = biomes.GetComponentsInChildren<MeshRenderer> () [10].sharedMaterial.mainTexture as Texture2D;
			rainForestText.wrapMode = TextureWrapMode.Clamp;
		}
			
		Refresh();
	}
		
	//Refresh called by the inspector on resolution Changes and onEnable
	public void Refresh () {
		if (resolution != currentResolution || currentSize != chunkSize) {
			CreateGrid();
		}

		if (tex.width != resolution || huTex.width != resolution || tempTex.width != resolution || huGrayTex.width != resolution || tempGrayTex.width != resolution) {
			resizeTextures ();
			print("Resizing Height, Humidity and Temperature Maps");
		}
			
		Quaternion q = Quaternion.Euler (rotation);
		Vector3 point00 = q *transform.TransformPoint(new Vector3(-0.5f,-0.5f)) + offset ;
		Vector3 point10 = q * transform.TransformPoint(new Vector3( 0.5f,-0.5f)) + offset ;
		Vector3 point01 = q * transform.TransformPoint(new Vector3(-0.5f, 0.5f)) + offset ;
		Vector3 point11 = q * transform.TransformPoint(new Vector3( 0.5f, 0.5f)) + offset ;

		Vector3 point001 = q *transform.TransformPoint(new Vector3(-0.5f,-0.5f)) + randomOffset ;
		Vector3 point101 = q * transform.TransformPoint(new Vector3( 0.5f,-0.5f)) + randomOffset ;
		Vector3 point011 = q * transform.TransformPoint(new Vector3(-0.5f, 0.5f)) + randomOffset ;
		Vector3 point111 = q * transform.TransformPoint(new Vector3( 0.5f, 0.5f)) + randomOffset ;

		NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];
		float stepSize = 1f / resolution;
		for (int v = 0, y = 0; y <= resolution; y++) {
			Vector3 point0 = Vector3.Lerp(point00, point01, y * stepSize);
			Vector3 point1 = Vector3.Lerp(point10, point11, y * stepSize);

			Vector3 randomPoint0 = Vector3.Lerp(point001, point011, y * stepSize);
			Vector3 randomPoint1 = Vector3.Lerp(point101, point111, y * stepSize);

			for (int x = 0; x <= resolution; x++, v++) {
				Vector3 point = Vector3.Lerp(point0, point1, x * stepSize);
				Vector3 rpoint = Vector3.Lerp(randomPoint0, randomPoint1, x * stepSize);
				float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
				float randomSample = Noise.Sum(method, rpoint, frequency, octaves, lacunarity, persistence);
				sample = type == NoiseMethodType.Value ? (sample - 0.5f) : (sample * 0.5f);
				if (strenghtModifiesColors) {
					Color huColor = humidityColoring.Evaluate (sample + 0.5f);
					Color tempColor = temperatureColoring.Evaluate (sample + 0.5f);

					colors[v] = coloring.Evaluate(sample + 0.5f);
					tex.SetPixel (x, y,	new Color((sample + 0.5f),(sample + 0.5f),(sample + 0.5f)));
					huGrayTex.SetPixel (x, y,	new Color(1f-huColor.grayscale, 1f-huColor.grayscale, 1f-huColor.grayscale));
					tempGrayTex.SetPixel (x, y,	new Color(tempColor.grayscale, tempColor.grayscale, tempColor.grayscale));
					huTex.SetPixel (x, y,	humidityColoring.Evaluate (sample + 0.5f));
					tempTex.SetPixel (x, y,	temperatureColoring.Evaluate (1-(sample + 0.5f)));
					sample *= strength;

				} else {
					sample *= strength;
					Color huColor = humidityColoring.Evaluate (sample + 0.5f);
					Color tempColor = temperatureColoring.Evaluate (sample + 0.5f);

					colors[v] = coloring.Evaluate(sample + 0.5f);
					tex.SetPixel (x, y,	new Color((sample + 0.5f),(sample + 0.5f),(sample + 0.5f)));
					huGrayTex.SetPixel (x, y,	new Color(1f-huColor.grayscale, 1f-huColor.grayscale, 1f-huColor.grayscale));
					tempGrayTex.SetPixel (x, y,	new Color(tempColor.grayscale, tempColor.grayscale, tempColor.grayscale));
					huTex.SetPixel (x, y,	humidityColoring.Evaluate (sample + 0.5f));
					tempTex.SetPixel (x, y,	temperatureColoring.Evaluate (1-(sample + 0.5f)));
				
				}

				vertices [v].y = sample * chunkSize;
			}
		}
		tempGrayTex.Apply ();
		huGrayTex.Apply ();
		tex.Apply ();
		huTex.Apply ();
		tempTex.Apply();

		StartCoroutine (SaveTextureToFile(tex));
		StartCoroutine (SaveTextureToFile(huTex));
		StartCoroutine (SaveTextureToFile(tempTex));
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		generateBiomes ();
		mesh.colors = colors;

		//WaterPlane resizing and tiling
		float ratio = chunkSize / 10f; //How should the water plane be scaled
		waterPlane.transform.localScale = new Vector3(ratio, 1f, ratio);
		waterPlane.transform.position = this.transform.position + new Vector3(0f,-ratio - 1,0f); //Centering plane
	}

	public void randomHumidityTemperature(){
		painterScript.GetComponentInChildren<TexturePainter> ().clearCanvasBrushes ();
		huTex.name = "HumidityMap";
		tempTex.name = "TemperatureText";
		huTex.wrapMode = TextureWrapMode.Clamp;
		tempTex.wrapMode = TextureWrapMode.Clamp;
		huTex.filterMode = FilterMode.Trilinear;
		tempTex.filterMode = FilterMode.Trilinear;
		huTex.anisoLevel = 0;
		tempTex.anisoLevel = 0;
		huPainter.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTexture = huTex;
		tPainter.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTexture = tempTex;

		if (huTex.width != resolution || tempTex.width != resolution) {
			huTex.Resize(resolution,resolution);
			tempTex.Resize (resolution, resolution);
			print("Resizing Height and Humidity Maps");
		}

		if (fixSeed) {
			Random.seed = randomSeed;
		} else {
			randomSeed = Random.seed;
		}

		float randomFOffset = (Random.value -0.5f) * 4;
		float randomOcOffset = (Random.value -0.5f) * 4;
		float randomPerOffset = (Random.value -0.8f);
		randomOffset = new Vector3 (Random.value * 360, Random.value * 360, Random.value * 360);
		randomHuValue = Random.value;
		randomTempValue = Random.value;

		Quaternion q = Quaternion.Euler (rotation);
		Vector3 point00 = q *transform.TransformPoint(new Vector3(-0.5f,-0.5f)) + randomOffset;
		Vector3 point10 = q * transform.TransformPoint(new Vector3( 0.5f,-0.5f)) + randomOffset;
		Vector3 point01 = q * transform.TransformPoint(new Vector3(-0.5f, 0.5f)) + randomOffset;
		Vector3 point11 = q * transform.TransformPoint(new Vector3( 0.5f, 0.5f)) + randomOffset;

		NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];
		float stepSize = 1f / resolution;
		for (int v = 0, y = 0; y <= resolution; y++) {
			Vector3 point0 = Vector3.Lerp(point00, point01, y * stepSize);
			Vector3 point1 = Vector3.Lerp(point10, point11, y * stepSize);
			for (int x = 0; x <= resolution; x++, v++) {
				Vector3 point = Vector3.Lerp(point0, point1, x * stepSize);
				float sample = Noise.Sum(method, point, frequency + randomFOffset, octaves + randomOcOffset, lacunarity, persistence + randomPerOffset);
				sample = type == NoiseMethodType.Value ? (sample - 0.5f) : (sample * 0.5f);

				Color huColor = humidityColoring.Evaluate (sample + randomHuValue);
				Color tempColor = temperatureColoring.Evaluate (sample + randomTempValue);

				huGrayTex.SetPixel (x, y,	new Color(1f-huColor.grayscale, 1f-huColor.grayscale, 1f-huColor.grayscale));
				tempGrayTex.SetPixel (x, y,	new Color(1f-tempColor.grayscale, 1f-tempColor.grayscale, 1f-tempColor.grayscale));
				huTex.SetPixel (x, y,	humidityColoring.Evaluate (sample + randomHuValue));
				tempTex.SetPixel (x, y,	temperatureColoring.Evaluate (sample + randomTempValue));
			}
		}
		huGrayTex.Apply();
		tempGrayTex.Apply();
		huTex.Apply ();
		tempTex.Apply ();
		generateBiomes ();
	}

	private void CreateGrid () {
		resizeTextures ();
		currentResolution = resolution;
		currentSize = chunkSize;
		mesh.Clear();
		vertices = new Vector3[(resolution + 1) * (resolution + 1)];
		normals = new Vector3[vertices.Length];
		colors = new Color[vertices.Length];
		Vector2[] uv = new Vector2[vertices.Length];
		int[] triangles = new int[resolution * resolution * 6];

		float stepSize = chunkSize / resolution;
		float uvStep = 1f / resolution;
		for (int v = 0, z =0; z <= resolution; z++) {
			for (int x = 0; x <= resolution; x++, v++) {
				vertices[v] = new Vector3(x * stepSize - 0.5f*chunkSize, 0f, z * stepSize - 0.5f*chunkSize);
				uv[v] = new Vector2(x * uvStep, z * uvStep);
				normals[v] = Vector3.up;
				colors[v] = Color.black;
			}
		}

		for (int t = 0, v = 0, y = 0; y < resolution; y++, v++) {
			for (int x = 0; x < resolution; x++, v++, t += 6) {
				triangles[t] = v;
				triangles[t + 1] = v + resolution + 1;
				triangles[t + 2] = v + 1;
				triangles[t + 3] = v + 1;
				triangles[t + 4] = v + resolution + 1;
				triangles[t + 5] = v + resolution + 2;
			}
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;
		mesh.normals = normals;
		mesh.colors = colors;
		MeshCollider col = GetComponentInParent<MeshCollider> ();
		col.sharedMesh = mesh;
		col.convex = true;
		col.convex = false;
	}

	public void updateHeight(){
		int localRes = resolution;
		RenderTexture heightRt = hPainter.GetComponentsInChildren<Camera> ()[1].targetTexture;

		if (heightRt.width != resolution) {
			RenderTexture newResH = RenderTexture.GetTemporary (localRes, localRes);
			RenderTexture.active = newResH;
			Graphics.Blit (heightRt, newResH);
			Texture2D nt = new Texture2D (localRes, localRes, TextureFormat.ARGB32, false);
			nt.ReadPixels (new Rect (0, 0, localRes, localRes), 0, 0);
			nt.Apply ();


			for (int v = 0, y = 0; y <= resolution; y++) {
				for (int x = 0; x <= resolution; x++, v++) {
					float sample = nt.GetPixel(x,y).r;
					vertices [v].y = (sample-0.5f) * chunkSize;
				}
			}

			mesh.vertices = vertices;
			mesh.RecalculateNormals();

		} else {

			RenderTexture.active = heightRt;
			Texture2D hTexture = new Texture2D(localRes, localRes, TextureFormat.ARGB32, false);
			hTexture.wrapMode = TextureWrapMode.Clamp;
			hTexture.ReadPixels (new Rect (0, 0, localRes, localRes), 0, 0);
			hTexture.Apply ();

			for (int v = 0, y = 0; y <= resolution; y++) {
				for (int x = 0; x <= resolution; x++, v++) {
					float sample = hTexture.GetPixel(x,y).r;
					vertices [v].y = (sample-0.5f) * chunkSize;
				}
			}

			mesh.vertices = vertices;
			mesh.RecalculateNormals();
		}
	}

	public void updateGrayscales(){
		int localRes = resolution;
		RenderTexture humidtyRT = huPainter.GetComponentsInChildren<Camera> ()[1].targetTexture;
		RenderTexture temperatureRT = tPainter.GetComponentsInChildren<Camera> ()[1].targetTexture;


		if (humidtyRT.width != resolution) {
			RenderTexture newResHuRt = RenderTexture.GetTemporary (localRes, localRes);
			RenderTexture.active = newResHuRt;
			Graphics.Blit (humidtyRT, newResHuRt);
			Texture2D nt = new Texture2D (localRes, localRes, TextureFormat.ARGB32, false);
			nt.ReadPixels (new Rect (0, 0, localRes, localRes), 0, 0);
			nt.Apply ();

			RenderTexture newResTemp = RenderTexture.GetTemporary (localRes, localRes);
			RenderTexture.active = newResTemp;
			Graphics.Blit (temperatureRT, newResTemp);
			Texture2D nt2 = new Texture2D (localRes, localRes, TextureFormat.ARGB32, false);
			nt2.ReadPixels (new Rect (0, 0, localRes, localRes), 0, 0);
			nt2.Apply ();

			huGrayTex = new Texture2D (localRes, localRes, TextureFormat.ARGB32, false);
			tempGrayTex = new Texture2D (localRes, localRes, TextureFormat.ARGB32, false);
			huGray.GetComponent<MeshRenderer> ().sharedMaterial.mainTexture = huGrayTex;
			tempGray.GetComponent<MeshRenderer> ().sharedMaterial.mainTexture = tempGrayTex;

			for (int x = 0, v = 0; x < localRes; x++) {
				for (int y = 0; y < localRes; y++, v++) {
					Color hu = nt.GetPixel (x, y);
					Color temp = nt2.GetPixel (x, y);
					huGrayTex.SetPixel (x, y,	new Color (1f - hu.grayscale, 1f - hu.grayscale, 1f - hu.grayscale));
					tempGrayTex.SetPixel (x, y,	new Color (1f - temp.grayscale, 1f - temp.grayscale, 1f - temp.grayscale));
				}
			}

			huGrayTex.Apply ();
			tempGrayTex.Apply ();
			Invoke ("generateBiomes", 0.1f);
		} else {
			
			RenderTexture.active = humidtyRT;
			Texture2D huTexture = new Texture2D(localRes, localRes, TextureFormat.ARGB32, false);
			huTexture.wrapMode = TextureWrapMode.Clamp;
			huTexture.ReadPixels (new Rect (0, 0, localRes, localRes), 0, 0);
			huTexture.Apply ();

			RenderTexture.active = temperatureRT;
			Texture2D tempTexture = new Texture2D(localRes, localRes, TextureFormat.ARGB32, false);  
			tempTexture.wrapMode = TextureWrapMode.Clamp;
			tempTexture.ReadPixels (new Rect (0, 0, localRes, localRes), 0, 0);
			tempTexture.Apply ();

			huGrayTex = new Texture2D(localRes,localRes,TextureFormat.ARGB32, false);
			tempGrayTex = new Texture2D(localRes,localRes,TextureFormat.ARGB32, false);
			huGray.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = huGrayTex;
			tempGray.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = tempGrayTex;

			for (int x = 0, v = 0; x < localRes; x++){
				for(int y = 0; y < localRes; y++, v++){
					Color hu = huTexture.GetPixel(x,y);
					Color temp = tempTexture.GetPixel(x,y);
					huGrayTex.SetPixel (x, y,	new Color(1f-hu.grayscale, 1f-hu.grayscale, 1f-hu.grayscale));
					tempGrayTex.SetPixel (x, y,	new Color(1f-temp.grayscale, 1f-temp.grayscale, 1f-temp.grayscale));
				}
			}

			huGrayTex.Apply ();
			tempGrayTex.Apply ();
			Invoke ("generateBiomes", 0.1f);
		}
	}

	public void generateBiomes(){
		//Load all textures
		RenderTexture humidtyRT = huPainter.GetComponentsInChildren<Camera> ()[1].targetTexture;
		RenderTexture temperatureRT = tPainter.GetComponentsInChildren<Camera> ()[1].targetTexture;
	
		RenderTexture.active = humidtyRT;
		Texture2D humidityTexture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);   
		humidityTexture.ReadPixels (new Rect (0, 0, resolution, resolution), 0, 0);
		humidityTexture.Apply ();

		RenderTexture.active = temperatureRT;
		Texture2D temperatureTexture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);   
		temperatureTexture.ReadPixels (new Rect (0, 0, resolution, resolution), 0, 0);
		temperatureTexture.Apply ();

		for (int x = 0, v = 0; x < resolution; x++){
			for(int y = 0; y < resolution; y++, v++){
				Color hu = huGrayTex.GetPixel(x,y);
				Color temp = tempGrayTex.GetPixel(x,y);
				colors [v] = hu;
				int biome = getBiome (hu.r * 300, temp.r * 300);
				paintSplatForBiome( biome ,x,y);
			}
		}

		polarText.Apply ();
		tundraText.Apply ();
		borealText.Apply ();
		coldDesertText.Apply ();
		praireText.Apply ();
		tempForestText.Apply ();
		warmDesertText.Apply ();
		grasslandText.Apply ();
		savanaText.Apply ();
		tropForestText.Apply ();
		rainForestText.Apply ();
	}

	//1 - polar, 2 - tundra, 3 - boreal, 4 - cold desert
	//5 - praire, 6 - temp. forest, 7 -warm desert
	//8 - grassland, 9 - savana, 10 - trop forest, 11- trop rain forest
	private int getBiome(float temperature, float humidity) {
		if (temperature > 220) {
			return 1;
		} else if (temperature > 180) {
			return 2;
		} else if (temperature > 130) {
			return 3;
		} else if (temperature > 100) {
			if (humidity > 160) {
				return 6;
			} else if (humidity > 100) {
				return 5;
			} else {
				return 4;
			}
		} else {
			if (humidity > 200) {
				return 11;
			} else if (humidity > 160) {
				return 10;
			} else if (humidity > 113) {
				return 9;
			} else if (humidity > 73) {
				return 8;
			} else {
				return 7;
			}
		}
	}

	//1 - polar, 2 - tundra, 3 - boreal, 4 - cold desert
	//5 - praire, 6 - temp. forest, 7 -warm desert
	//8 - grassland, 9 - savana, 10 - trop forest, 11- trop rain forest
	private void paintSplatForBiome(int biome, int onXPos, int onYPos){
		Color color;
		color = polarText.GetPixel (onXPos, onYPos);
		if (biome == 1) {
			polarText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,1f));
		} else {
			polarText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,0f));
		}
		color = tundraText.GetPixel (onXPos, onYPos);
		if (biome == 2) {
			tundraText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,1f));
		} else {
			tundraText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,0f));
		}
		color = borealText.GetPixel (onXPos, onYPos);
		if (biome == 3) {
			borealText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,1f));
		} else {
			borealText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,0f));
		}
		color = coldDesertText.GetPixel (onXPos, onYPos);
		if (biome == 4) {
			coldDesertText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,1f));
		} else {
			coldDesertText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,0f));
		}
		color = praireText.GetPixel (onXPos, onYPos);
		if (biome == 5) {
			praireText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,1f));
		} else {
			praireText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,0f));
		}
		color = tempForestText.GetPixel (onXPos, onYPos);
		if (biome == 6) {
			tempForestText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,1f));
		} else {
			tempForestText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,0f));
		}
		color = warmDesertText.GetPixel (onXPos, onYPos);
		if (biome == 7) {
			warmDesertText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,1f));
		} else {
			warmDesertText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,0f));
		}
		color = grasslandText.GetPixel (onXPos, onYPos);
		if (biome == 8) {
			grasslandText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,1f));
		} else {
			grasslandText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,0f));
		}
		color = savanaText.GetPixel (onXPos, onYPos);
		if (biome == 9) {
			savanaText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,1f));
		} else {
			savanaText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,0f));
		}
		color = tropForestText.GetPixel (onXPos, onYPos);
		if (biome == 10) {
			tropForestText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,1f));
		} else {
			tropForestText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,0f));
		}
		color = rainForestText.GetPixel (onXPos, onYPos);
		if (biome == 11) {
			rainForestText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,1f));
		} else {
			rainForestText.SetPixel (onXPos, onYPos, new Color(color.r,color.g,color.b,0f));
		}
	}

	private void resizeTextures(){
		tex.Resize(resolution,resolution);
		huTex.Resize (resolution, resolution);
		tempTex.Resize (resolution, resolution);
		tempGrayTex.Resize (resolution, resolution);
		huGrayTex.Resize (resolution, resolution);
		float size = resolution / 508.0f;
		BiomesCamera.orthographicSize = size;
		Vector3 newCameraLocalPos = new Vector3 (-0.5f + size, -0.5f + size, -3.27f);
		BiomesCamera.transform.localPosition = newCameraLocalPos;
	}

	IEnumerator SaveTextureToFile(Texture2D savedTexture){    
		string fullPath=System.IO.Directory.GetCurrentDirectory()+"/Assets/Terrain Creator/Textures/";
		System.DateTime date = System.DateTime.Now;
		string fileName = "HeighthMapTexture.png";
		if (!System.IO.Directory.Exists(fullPath))    
			System.IO.Directory.CreateDirectory(fullPath);
		var bytes = savedTexture.EncodeToPNG();
		System.IO.File.WriteAllBytes(fullPath+fileName, bytes);
		Debug.Log ("<color=orange> HeightMap Texture Saved</color>"+fullPath+fileName);
		yield return null;
	}

	void Start () {
		
	}

	void Update () {
		
	}
}
