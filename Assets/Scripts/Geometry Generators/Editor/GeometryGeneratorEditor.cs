using UnityEngine;
using UnityEditor;

namespace GeometryGenerators {
    
    public abstract class GeometryGeneratorEditor : Editor {

        public override void OnInspectorGUI () {
            EditorTools.DrawScriptReference(target);
            serializedObject.Update();
            DrawInspectorTargets();
            if(target is GeometryGeneratorWithGizmos){
                DrawGizmoProperties();
            }
            DrawOwnProperties();
            DrawButtons();
        }

        protected abstract void DrawOwnProperties ();

        protected void DrawInspectorTargets () {
            var selfProp = serializedObject.FindProperty("targetOnlySelf");
            EditorGUILayout.PropertyField(selfProp);
            if(!selfProp.boolValue){
                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetMeshFilters"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetMeshColliders"), true);
            }
        }

        protected void DrawButtons () {
            GUILayout.Space(10f);
            if(target is GeometryGenerator generator){
                if(GUILayout.Button("Generate")){
                    generator.Generate();
                }
                if(GUILayout.Button("Clear")){
                    generator.Clear();
                }
            }else{
                GUILayout.Label($"ERROR! {target.GetType()} is not a {nameof(GeometryGenerator)}!");
            }
        }

        protected void DrawGizmoProperties () {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("drawGizmos"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gizmoColor"));
        }
        
    }
	
}
