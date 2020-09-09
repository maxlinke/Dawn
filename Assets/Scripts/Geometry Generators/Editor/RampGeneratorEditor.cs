using UnityEditor;

namespace GeometryGenerators {
    
    [CustomEditor(typeof(RampGenerator))]
    public class RampGeneratorEditor : GeometryGeneratorEditor {

        protected override void DrawOwnProperties () {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("multiMaterial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("frontAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rearAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("frontLength"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rearLength"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
        }
        
    }
	
}
