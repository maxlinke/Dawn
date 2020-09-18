using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RigidbodyDragHelper))]
public class RigidbodyDragHelperEditor : Editor {

    const string headerText = "Use this to calculate a rigidbody's terminal velocity or what drag to use for a particular terminal velocity";
    const string helpText = @"Help

- ρ(Air): 1.27 kg/m³
- ρ(Water): 1000 kg/m³

- cD(Sphere): 0.47
- cD(Half Sphere): 0.42
- cD(Cone): 0.5
- cD(Cube Flat): 1.05
- cD(Cube 45°): 0.8
- cD(Long Cylinder): 0.82
- cD(Short Cylinder): 1.15
- cD(Streamlined): 0.04";

    string unityOutput = "output";
    string realOutput = "output";
    string realSubText = null;

    public override void OnInspectorGUI () {
        GUILayout.Space(10f);
        EditorTools.TextBoxCentered(headerText);
        GUILayout.Space(10f);

        serializedObject.Update();
        DrawGeneralSettings(out var gravity, out var terminalVelocity);
        DrawUnitySettings();
        DrawRealSettings();
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        GUILayout.Space(10f);
        EditorTools.TextBox(helpText);

        float DrawField (string propName) {
            var prop = serializedObject.FindProperty(propName);
            EditorGUILayout.PropertyField(prop, true);
            return prop.floatValue;
        }

        void DrawOutputLabel (string text, string subText = null) {
            GUILayout.Space(10f);
            EditorTools.DrawCentered(() => {
                GUILayout.Label(text);
            });
            if(subText != null){
                EditorTools.DrawCentered(() => {
                    GUILayout.Label(subText, EditorStyles.miniLabel);
                });
            }
        }

        void DrawGeneralSettings (out float outputGravity, out float outputTerminalVelocity) {
            outputGravity = DrawField("gravity");
            outputTerminalVelocity = DrawField("terminalVelocity");
        }

        void DrawUnitySettings () {
            var drag = DrawField("drag");
            var deltaTime = DrawField("deltaTime");
            GUILayout.Space(10f);
            EditorTools.DrawHorizontal(() => {
                if(GUILayout.Button("Terminal Velocity")){
                    var vt = RigidbodyDragHelper.CalculateTerminalVelocity(drag, gravity, deltaTime);
                    unityOutput = $"vt(d, g, dt) = {vt} {RigidbodyDragHelper.u_vt}";
                }
                if(GUILayout.Button("          Drag          ")){
                    var d = RigidbodyDragHelper.CalculateDrag(terminalVelocity, gravity, deltaTime);
                    unityOutput = $"d(vt, g, dt) = {d} {RigidbodyDragHelper.u_d}";
                }
            });
            DrawOutputLabel(unityOutput);            
        }

        void DrawRealSettings () {
            var mass = DrawField("mass");
            var airDensity = DrawField("airDensity");
            var dragCoefficient = DrawField("dragCoefficient");
            var area = DrawField("area");
            GUILayout.Space(10f);
            EditorTools.DrawHorizontal(() => {
                if(GUILayout.Button("Terminal Velocity")){
                    var vt = RigidbodyDragHelper.CalculateRealTerminalVelocity(mass, gravity, airDensity, dragCoefficient, area);
                    realOutput = $"vt(m, g, ρ, cD, A) = {vt} {RigidbodyDragHelper.u_vt}";
                    realSubText = null;
                }
                if(GUILayout.Button("Drag Coefficient")){
                    var cD = RigidbodyDragHelper.CalculateRealDragCoefficient(mass, gravity, airDensity, terminalVelocity, area);
                    realOutput = $"cD(m, g, ρ, A, vt) = {cD} {RigidbodyDragHelper.u_cD}";
                    realSubText = null;
                }
                if(GUILayout.Button("         Area         ")){
                    var a = RigidbodyDragHelper.CalculateRealArea(mass, gravity, airDensity, dragCoefficient, terminalVelocity);
                    var r = Mathf.Sqrt(a / Mathf.PI);
                    realOutput = $"a(m, g, ρ, cD, vt) = {a} {RigidbodyDragHelper.u_a}";
                    realSubText = $"(radius = {r} {RigidbodyDragHelper.u_r})";
                }
            });
            DrawOutputLabel(realOutput, realSubText);
        }

    }
	
}
