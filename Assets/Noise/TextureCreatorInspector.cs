using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TextureCreator))]
public class TextureCreatorInspector : Editor {
	private TextureCreator creator;

	public override void OnInspectorGUI ()
	{
		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector ();
		if (EditorGUI.EndChangeCheck()) {
			RefreshCreator();
		}
	}

	private void OnEnable () {
		creator = target as TextureCreator;
		Undo.undoRedoPerformed += RefreshCreator;
	}
	
	private void OnDisable () {
		Undo.undoRedoPerformed -= RefreshCreator;
	}

	private void RefreshCreator () {
		if (Application.isPlaying) {
			creator.FillTexture();
		}
	}
}
