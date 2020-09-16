using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RigidbodyDragHelper))]
public class RigidbodyDragHelperEditor : Editor {

    string label = string.Empty;

    public override void OnInspectorGUI () {
        // EditorTools.DrawScriptReference(this);

        GUILayout.Space(10f);
        EditorTools.DrawDisabled(() => {
            EditorTools.DrawCentered(() => {
                GUILayout.TextArea("Use this to calculate a rigidbody's terminal velocity or what drag to use for a particular terminal velocity");
            });
        });
        GUILayout.Space(10f);

        serializedObject.Update();
        var dragProp = serializedObject.FindProperty("drag");
        var gravProp = serializedObject.FindProperty("gravity");
        var dtProp = serializedObject.FindProperty("deltaTime");
        var vtProp = serializedObject.FindProperty("terminalVelocity");
        DrawField(dragProp, 0f, float.PositiveInfinity, out var drag);
        DrawField(gravProp, 0f, float.PositiveInfinity, out var gravity);
        DrawField(dtProp, 0.0001f, 1f, out var deltaTime);
        DrawField(vtProp, 0f, float.PositiveInfinity, out var terminalVelocity);
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        GUILayout.Space(10f);
        EditorTools.DrawHorizontal(() => {
            if(GUILayout.Button("Terminal Velocity")){
                terminalVelocity = RigidbodyDragHelper.CalculateTerminalVelocity(drag, gravity, deltaTime);
                label = $"vt(d, g, dt) = {terminalVelocity} m/s";
            }
            if(GUILayout.Button("          Drag          ")){
                drag = RigidbodyDragHelper.CalculateDrag(terminalVelocity, gravity, deltaTime);
                label = $"d(v, g, dt) = {drag}";
            }
        });

        GUILayout.Space(10f);
        EditorTools.DrawCentered(() => {
            GUILayout.Label(label);
        });

        void DrawField (SerializedProperty prop, float min, float max, out float output) {
            EditorGUI.BeginChangeCheck();
            var newVal = EditorGUILayout.FloatField(prop.displayName, Mathf.Clamp(prop.floatValue, min, max));
            if(EditorGUI.EndChangeCheck()){
                prop.floatValue = Mathf.Clamp(newVal, min, max);
            }
            output = prop.floatValue;
        }
    }
	
}
