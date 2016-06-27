using UnityEngine;
using System.Collections;

public class TextureCreator: MonoBehaviour {

	private Texture2D texture;
	public float frequency = 1f;

	[Range(1, 3)]
	public int dimensions = 3;

	[Range(2, 1024)]
	public int resolution = 256;

	[Range(1, 8)]
	public int octaves = 1;

	[Range(1f, 4f)]
	public float lacunarity = 2f;
	
	[Range(0f, 1f)]
	public float gain = 0.5f;

	public Gradient coloring;

	public NoiseMethodType type;

	private void OnEnable() {
		if (texture == null) {
			texture = new Texture2D (resolution, resolution, TextureFormat.RGB24, true);
			texture.name = "Procedural Noise";
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.filterMode = FilterMode.Trilinear;
			texture.anisoLevel = 9;
			this.GetComponent<MeshRenderer> ().material.mainTexture = texture;
			this.FillTexture();
		}
	}

	void Start () {
	
	}

	private void Update () {
		if (transform.hasChanged) {
			transform.hasChanged = false;
			FillTexture();
		}
	}

	public void FillTexture(){

		Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f,-0.5f));
		Vector3 point10 = transform.TransformPoint(new Vector3( 0.5f,-0.5f));
		Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
		Vector3 point11 = transform.TransformPoint(new Vector3( 0.5f, 0.5f));

		if (texture.width != resolution) {
			texture.Resize(resolution,resolution);
		}

		NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];
		float stepSize = 1f / resolution;
		for (int i = 0;  i < resolution; i++) {
			Vector3 point0 = Vector3.Lerp(point00, point01, (i + 0.5f) * stepSize);
			Vector3 point1 = Vector3.Lerp(point10, point11, (i + 0.5f) * stepSize);
			for (int j = 0; j < resolution; j++) {
				Vector3 point = Vector3.Lerp(point0, point1, (j + 0.5f) * stepSize);
				float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, gain);
				if (type != NoiseMethodType.Value) {
					sample = sample * 0.5f + 0.5f;
				}
				texture.SetPixel(j, i,	new Color(sample, sample, sample));
			}
		}
		texture.Apply();
	}
}
