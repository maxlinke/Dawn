using UnityEngine;
using UnityEditor;

namespace GeometryGenerators {
    
    public abstract class GeometryGeneratorEditor : GenericEditor {

        SerializedProperty targetSelfProp;

        protected virtual void OnEnable () {
            targetSelfProp = serializedObject.FindProperty("targetOnlySelf");
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();
            DrawButtons();
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            switch(property.name){
                case "targetMeshFilters":
                    if(!targetSelfProp.boolValue) EditorGUILayout.PropertyField(property);
                    return true;
                case "targetMeshColliders":
                    if(!targetSelfProp.boolValue) EditorGUILayout.PropertyField(property);
                    return true;
                default:
                    return false;
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
                if(GUILayout.Button("Save mesh as asset")){
                    generator.SaveMeshAsAsset();
                }
            }else{
                GUILayout.Label($"ERROR! {target.GetType()} is not a {nameof(GeometryGenerator)}!");
            }
        }
        
    }

    [CustomEditor(typeof(LadderGenerator))]
    public class LadderGeneratorEditor : GeometryGeneratorEditor { }

    [CustomEditor(typeof(RampGenerator))]
    public class RampGeneratorEditor : GeometryGeneratorEditor { }

    [CustomEditor(typeof(StairGenerator))]
    public class StairGeneratorEditor : GeometryGeneratorEditor { }
	
}
