using UnityEditor;

namespace GeometryGenerators {

    [CustomEditor(typeof(PlaneGenerator))]
    public class PlaneGeneratorEditor : GeometryGeneratorEditor {

        protected override void DrawOwnProperties () {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("xTiles"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("zTiles"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tileSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("uvMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("uvScale"));
        }
        
    }

}