using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaterPhysics))]
public class WaterPhysicsEditor : Editor {

    bool enableDensityCalcEditing = false;
    Rigidbody testRB = null;
    float testMass = 1f;
    float testDrag = 1f;

    WaterPhysics wps;

    void OnEnable () {
        wps = target as WaterPhysics;
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        var densProp = FindProp("m_density");
        var viscProp = FindProp("m_viscosity");
        var buoyLimitProp = FindProp("m_buoyancyLimit");
        var buoyTypeProp = FindProp("m_useSimpleBuoyancy");
        var buoyDepthProp = FindProp("m_simpleBuoyancyNeutralizationRange");
        var densGravProp = FindProp("m_densityCalcGravity");
        var densAirProp = FindProp("m_densityCalcAirDensity");
        var densTimeProp = FindProp("m_densityCalcFixedDeltaTime");
        
        EditorTools.DrawScriptReference(this);
        GUILayout.Space(10f);
        DrawReset();
        DrawProps();
        DrawCalculator();

        serializedObject.ApplyModifiedProperties();

        SerializedProperty FindProp (string name) {
            return serializedObject.FindProperty(name);
        }

        void DrawReset () {
            if(EditorTools.ButtonCentered("Reset", 200f, true)){
                densProp.floatValue = WaterPhysics.DEFAULT_DENSITY;
                viscProp.floatValue = WaterPhysics.DEFAULT_VISCOSITY;
                buoyLimitProp.floatValue = WaterPhysics.DEFAULT_BUOYANCY_LIMIT;
                buoyTypeProp.boolValue = WaterPhysics.DEFAULT_SIMPLE_BUOYANCY;
                buoyDepthProp.floatValue = WaterPhysics.DEFAULT_SIMPLE_BUOYANCY_NEUTRALIZATION_RANGE;
                densGravProp.floatValue = WaterPhysics.DEFAULT_GRAVITY;
                densAirProp.floatValue = WaterPhysics.DEFAULT_AIR_DENSITY;
                densTimeProp.floatValue = WaterPhysics.DEFAULT_FIXED_DELTA_TIME;
            }
        }

        void DrawProps () {
            EditorGUILayout.PropertyField(densProp);
            EditorGUILayout.PropertyField(viscProp, new GUIContent($"{viscProp.displayName} (Drag)"));
            EditorGUILayout.PropertyField(buoyLimitProp);
            EditorGUILayout.PropertyField(buoyTypeProp);
            EditorGUILayout.PropertyField(buoyDepthProp);
            GUILayout.Space(10f);
            var gc = GUI.enabled;
            GUI.enabled = enableDensityCalcEditing;
            EditorGUILayout.PropertyField(densGravProp);
            EditorGUILayout.PropertyField(densAirProp);
            EditorGUILayout.PropertyField(densTimeProp);
            GUI.enabled = gc;             
            enableDensityCalcEditing = EditorGUILayout.Toggle("Enable Editing", enableDensityCalcEditing);
        }

        void DrawCalculator () {
            GUILayout.Space(10f);
            EditorTools.HeaderLabel("Buoyancy Calculator");
            testRB = (Rigidbody)EditorGUILayout.ObjectField("Rigidbody", testRB, typeof(Rigidbody), false);
            var gc = GUI.enabled;
            if(testRB != null){
                GUI.enabled = false;
                testMass = testRB.mass;
                testDrag = testRB.drag;
            }
            testMass = Mathf.Max(0.0001f, EditorGUILayout.FloatField("Mass", testMass));
            testDrag = Mathf.Max(0f,      EditorGUILayout.FloatField("Drag", testDrag));
            GUI.enabled = gc;
            var approxDens = wps.ApproxDensity(testMass, testDrag);
            var approxBuoy = wps.BuoyancyFromDensity(approxDens);
            var approxBuoyUnclamped = wps.UnclampedBuoyancyFromDensity(approxDens);
            EditorTools.LabelWithLabel("Approx. Density", $"{approxDens} g/cm³");
            if(approxBuoy > approxBuoyUnclamped){
                EditorTools.LabelWithLabel("Buoyancy", $"{approxBuoy} (raw: {approxBuoyUnclamped})");
            }else{
                EditorTools.LabelWithLabel("Buoyancy", approxBuoy.ToString());
            }
        }

    }
	
}
