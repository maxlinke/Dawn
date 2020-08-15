using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor {

	public override void OnInspectorGUI () {
		TerrainGenerator tg = target as TerrainGenerator;

//		DrawDefaultInspector();
		DrawCustomInspector(tg);

		if(GUILayout.Button("Generate")){
			tg.Generate();
		}
	}

	void DrawScriptReference (TerrainGenerator tg) {
		GUI.enabled = false;
		MonoScript script = MonoScript.FromMonoBehaviour(tg);
		EditorGUILayout.ObjectField("Script", script, script.GetType(), false);
		GUI.enabled = true;
	}

	void DrawCustomInspector (TerrainGenerator tg) {
		serializedObject.Update();
		DrawScriptReference(tg);
		//components
		EditorGUILayout.PropertyField(serializedObject.FindProperty("mf"), new GUIContent("MeshFilter"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("mc"), new GUIContent("MeshCollider"), true);
		//general settings
		EditorGUILayout.PropertyField(serializedObject.FindProperty("seed"), new GUIContent("Seed"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("xTiles"), new GUIContent("Number of tiles in X"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("zTiles"), new GUIContent("Number of tiles in Z"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("tileSize"), new GUIContent("Tile Size"), true);
		SerializedProperty uvModeProp = serializedObject.FindProperty("uvMode");
		EditorGUILayout.PropertyField(uvModeProp, new GUIContent("UV Mode"), true);
		if(uvModeProp.enumValueIndex == (int)TerrainGenerator.UVMode.VERTEXCOORDS){
			EditorGUILayout.PropertyField(serializedObject.FindProperty("uvScale"), new GUIContent("UV Scale"), true);
		}else{
			GUI.enabled = false;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("uvScale"), new GUIContent("UV Scale"), true);
			GUI.enabled = true;
		}
		//deformation settings
		EditorGUILayout.PropertyField(serializedObject.FindProperty("deformationDirection"), new GUIContent("Deformation Direction"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("deformationStrength"), new GUIContent("Deformation Strength"), true);
		SerializedProperty noiseSourceProp = serializedObject.FindProperty("noiseSourceType");
		EditorGUILayout.PropertyField(noiseSourceProp, new GUIContent("UV Scale"), true);
		if(noiseSourceProp.enumValueIndex == (int)TerrainGenerator.NoiseSourceType.PERLIN){
			EditorGUILayout.PropertyField(serializedObject.FindProperty("perlinNoiseSources"), new GUIContent("Noise Sources"), true);
		}else if(noiseSourceProp.enumValueIndex == (int)TerrainGenerator.NoiseSourceType.TEXTURE){
			EditorGUILayout.PropertyField(serializedObject.FindProperty("textureNoiseSources"), new GUIContent("Noise Sources"), true);
		}
		serializedObject.ApplyModifiedProperties();
	}

}
