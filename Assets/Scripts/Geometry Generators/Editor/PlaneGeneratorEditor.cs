using UnityEditor;

namespace GeometryGenerators {

    [CustomEditor(typeof(PlaneGenerator))]
    public class PlaneGeneratorEditor : Editor {

        public override void OnInspectorGUI () {
            EditorTools.DrawScriptReference(target);

            serializedObject.Update();

            GeometryGenerator.DrawInspectorTargets(serializedObject);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("xTiles"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("zTiles"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tileSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("uvMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("uvScale"));
            DrawAdditionalProperties();

            serializedObject.ApplyModifiedProperties();

            GeometryGenerator.DrawButtons((GeometryGenerator)target);
        }

        protected virtual void DrawAdditionalProperties () { }
        
    }

}