using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaterPhyicsSettings))]
public class WaterPhysicsSettingsEditor : Editor {

    bool enableDensityCalcEditing = false;
    Rigidbody testRB = null;
    float testMass = 1f;
    float testDrag = 1f;

    WaterPhyicsSettings wps;

    void OnEnable () {
        wps = target as WaterPhyicsSettings;
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        var densProp = FindProp("m_density");
        var viscProp = FindProp("m_viscosity");
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
                densProp.floatValue = WaterPhyicsSettings.DEFAULT_DENSITY;
                viscProp.floatValue = WaterPhyicsSettings.DEFAULT_VISCOSITY;
                buoyTypeProp.boolValue = WaterPhyicsSettings.DEFAULT_SIMPLE_BUOYANCY;
                buoyDepthProp.floatValue = WaterPhyicsSettings.DEFAULT_SIMPLE_BUOYANCY_NEUTRALIZATION_RANGE;
                densGravProp.floatValue = WaterPhyicsSettings.DEFAULT_GRAVITY;
                densAirProp.floatValue = WaterPhyicsSettings.DEFAULT_AIR_DENSITY;
                densTimeProp.floatValue = WaterPhyicsSettings.DEFAULT_FIXED_DELTA_TIME;
            }
        }

        void DrawProps () {
            EditorGUILayout.PropertyField(densProp);
            EditorGUILayout.PropertyField(viscProp, new GUIContent($"{viscProp.displayName} (Drag)"));
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
            testMass = EditorGUILayout.FloatField("Mass", testMass);
            testDrag = EditorGUILayout.FloatField("Drag", testDrag);
            GUI.enabled = gc;
            var approxDens = wps.ApproxDensity(testMass, testDrag);
            var approxBuoy = wps.BuoyancyFromDensity(approxDens);
            EditorTools.LabelWithLabel("Approx. Density", $"{approxDens} g/cm³");
            EditorTools.LabelWithLabel("Buoyancy", approxBuoy.ToString());
        }

    }
	
}
