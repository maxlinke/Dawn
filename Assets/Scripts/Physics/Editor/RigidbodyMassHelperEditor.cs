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
        DrawSize(out var area, out var volume);
        serializedObject.ApplyModifiedProperties();

        DrawCalculatedResults();

        float DrawFloatField (string propName, string nameOverride = null) {
            var prop = serializedObject.FindProperty(propName);
            var label = nameOverride == null ? prop.displayName : nameOverride;
            EditorGUILayout.PropertyField(prop, new GUIContent(label), true);
            return prop.floatValue;
        }

        void DrawSize (out float outputArea, out float outputVolume) {
            outputArea = 0f;
            outputVolume = 0f;
            var prop = serializedObject.FindProperty("size");
            string displayName;
            float multiplier;
            System.Func<float, float> calcArea;
            System.Func<float, float> calcVolume;
            switch(shape){
                case RigidbodyMassHelper.Shape.Box:
                    displayName = "Length";
                    multiplier = 1f;
                    calcArea = RigidbodyMassHelper.CalculateSquareArea;
                    calcVolume = RigidbodyMassHelper.CalculateCubeVolume;
                    break;
                case RigidbodyMassHelper.Shape.Sphere:
                    displayName = "Diameter";
                    multiplier = 0.5f;
                    calcArea = RigidbodyMassHelper.CalculateCircleArea;
                    calcVolume = RigidbodyMassHelper.CalculateSphereVolume;
                    break;
                default:
                    displayName = null;
                    multiplier = 0f;
                    calcArea = null;
                    calcVolume = null;
                    break;
            }
            if(displayName == null){
                var msg = $"Unknown {nameof(RigidbodyMassHelper.Shape)} \"{shape}\"!";
                EditorGUILayout.LabelField(msg);
                Debug.LogError(msg);
                return;
            }
            EditorGUILayout.PropertyField(prop, new GUIContent(displayName), true);
            var size = multiplier * prop.floatValue;
            outputArea = calcArea(size);
            outputVolume = calcVolume(size);
        }

        void DrawCalculatedResults () {
            GUILayout.Space(4f);
            EditorTools.HeaderLabel("Results");
            EditorTools.LabelWithLabel("Area", $"{area} m²");
            EditorTools.LabelWithLabel("Volume", $"{volume} m³");
            var mass = RigidbodyMassHelper.CalculateMass(volume, density);
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