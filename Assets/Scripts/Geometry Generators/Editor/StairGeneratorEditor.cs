using UnityEditor;

namespace GeometryGenerators {
    
    [CustomEditor(typeof(StairGenerator))]
    public class StairGeneratorEditor : GeometryGeneratorEditor {
        
        protected override void DrawOwnProperties () {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stepCount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stepHeight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stepLength"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("endLength"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
        }
        
    }
	
}
