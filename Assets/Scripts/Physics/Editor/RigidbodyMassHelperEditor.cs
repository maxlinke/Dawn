using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RigidbodyMassHelper))]
public class RigidbodyMassHelperEditor : Editor {

    public override void OnInspectorGUI () {
        serializedObject.Update();
        var density = DrawFloatField("materialDensity");
        density *= DrawFloatField("percentSolid") / 100f;
        var shapeProp = serializedObject.FindProperty("shape");
        EditorGUILayout.PropertyField(shapeProp);
        var shape = (RigidbodyMassHelper.Shape)(shapeProp.enumValueIndex);
        DrawSize(out var volume, out var faceArea, out var sideArea);
        serializedObject.ApplyModifiedProperties();

        DrawCalculatedResults();

        float DrawFloatField (string propName, string nameOverride = null) {
            var prop = serializedObject.FindProperty(propName);
            var label = nameOverride == null ? prop.displayName : nameOverride;
            EditorGUILayout.PropertyField(prop, new GUIContent(label), true);
            return prop.floatValue;
        }

        void DrawSize (out float outputVolume, out float outputFaceArea, out float outputSideArea) {
            outputFaceArea = 0f;
            outputSideArea = -1f;
            outputVolume = 0f;
            float length, radius;
            switch(shape){
                case RigidbodyMassHelper.Shape.Box:
                    length = GetLength();
                    outputFaceArea = length * length;
                    outputVolume = length * outputFaceArea;
                    break;
                case RigidbodyMassHelper.Shape.Sphere:
                    radius = GetRadius();
                    outputFaceArea = Mathf.PI * radius * radius;
                    outputVolume = (4f / 3f) * outputFaceArea * radius;
                    break;
                case RigidbodyMassHelper.Shape.Cylinder:
                    length = GetLength();
                    radius = GetRadius();
                    outputFaceArea = Mathf.PI * radius * radius;
                    outputSideArea = 2f * radius * length;
                    outputVolume = outputFaceArea * length;
                    break;
                default:
                    var msg = $"Unknown {nameof(RigidbodyMassHelper.Shape)} \"{shape}\"!";
                    EditorGUILayout.LabelField(msg);
                    Debug.LogError(msg);
                    break;
            }

            float GetLength () {
                var lengthProp = serializedObject.FindProperty("length");
                EditorGUILayout.PropertyField(lengthProp);
                return lengthProp.floatValue;
            }

            float GetRadius () {
                var diameterProp = serializedObject.FindProperty("diameter");
                EditorGUILayout.PropertyField(diameterProp);
                return diameterProp.floatValue / 2f;
            }
        }

        void DrawCalculatedResults () {
            GUILayout.Space(4f);
            EditorTools.HeaderLabel("Results");
            if(sideArea == -1f){
                EditorTools.LabelWithLabel("Area", $"{faceArea} m²");
            }else{
                EditorTools.LabelWithLabel("Area (Face)", $"{faceArea} m²");
                EditorTools.LabelWithLabel("Area (Side)", $"{sideArea} m²");
            }
            EditorTools.LabelWithLabel("Volume", $"{volume} m³");
            var mass = volume * density * 1000f;
            string massString;
            if(mass > 1000f){
                massString = $"{(mass/1000f):F2} t";
            }else if(mass > 1f){
                massString = $"{mass:F2} kg";
            }else if(mass > 0.001f){
                massString = $"{(mass*1000f):F1} g";
            }else{
                massString = $"{(mass*1000f)} g";
            }
            EditorTools.LabelWithLabel("Mass", massString);
        }

    }
	
}