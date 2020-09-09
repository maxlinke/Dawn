using UnityEditor;

namespace GeometryGenerators {

    [CustomEditor(typeof(LadderGenerator))]
    public class LadderGeneratorEditor : GeometryGeneratorEditor {

        protected override void DrawOwnProperties () {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("originMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("length"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rungSpacing"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("topRungOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("railVertexCount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rungVertexCount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("railRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rungRadius"));
        }

    }

}
