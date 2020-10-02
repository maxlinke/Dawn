using UnityEngine;
using UnityEditor;

public static class EditorTools {

    public const float BACKGROUND_TINT_STRENGTH = 0.3f;

    public static void DrawScriptReference (UnityEngine.Object obj) {
        if(obj is MonoBehaviour monoBehaviour){
            DrawScriptReference(monoBehaviour);
        }else if(obj is ScriptableObject scriptableObject){
            DrawScriptReference(scriptableObject);
        }else{
            Debug.LogError($"Unknown type \"{obj.GetType()}\"!");
        }
    }

    public static void DrawScriptReference (MonoBehaviour script) {
        DrawScriptReference(MonoScript.FromMonoBehaviour(script), script.GetType());
    }

    public static void DrawScriptReference (ScriptableObject script) {
        DrawScriptReference(MonoScript.FromScriptableObject(script), script.GetType());
    }

    public static void DrawScriptReference (MonoScript script, System.Type origType) {
        var ge = GUI.enabled;
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", script, script.GetType(), false);
        GUI.enabled = ge;
    }

    public static void DrawProperty (SerializedProperty property) {
        EditorGUILayout.PropertyField(property, true);
    }

    public static void DrawObjectFieldWarnIfNull (SerializedProperty property) {
        var bgCol = GUI.backgroundColor;
        if(property.objectReferenceValue == null){
            GUI.backgroundColor = Color.Lerp(bgCol, Color.red, BACKGROUND_TINT_STRENGTH);
        }
        EditorGUILayout.PropertyField(property, true);
        GUI.backgroundColor = bgCol;
    }

    public static void DrawDisabled (System.Action drawAction) {
        var guiOn = GUI.enabled;
        GUI.enabled = false;
        drawAction();
        GUI.enabled = guiOn;
    }

    public static void DrawDisabled (SerializedProperty property) {
        DrawDisabled(() => DrawProperty(property));
    }

    public static void DrawHorizontal (System.Action drawAction) {
        GUILayout.BeginHorizontal();
        drawAction();
        GUILayout.EndHorizontal();
    }

    public static void DrawCentered (System.Action drawAction) {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        drawAction();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public static void DrawIndented (System.Action drawAction, int indentLevel = 1) {
        EditorGUI.indentLevel += indentLevel;
        drawAction();
        EditorGUI.indentLevel -= indentLevel;
    }

    public static void DrawIndented (SerializedProperty property, int indentLevel = 1) {
        DrawIndented(() => DrawProperty(property), indentLevel);
    }

    public static void DrawWithTintedBackground (System.Action drawAction, Color tintColor, float tintStrength = BACKGROUND_TINT_STRENGTH) {
        var bgCol = GUI.backgroundColor;
        GUI.backgroundColor = Color.Lerp(bgCol, tintColor, tintStrength);
        drawAction();
        GUI.backgroundColor = bgCol;
    }

    public static void DrawWithTintedBackground (SerializedProperty property, Color tintColor, float tintStrength = BACKGROUND_TINT_STRENGTH) {
        DrawWithTintedBackground(() => DrawProperty(property), tintColor, tintStrength);
    }

    public static void HeaderLabel (string text) {
        GUILayout.Label(text, EditorStyles.boldLabel);
    }

    public static void LabelWithLabel (string labelText, string contentText) {
        EditorTools.DrawHorizontal(() => {
            GUILayout.Label(labelText, GUILayout.Width(EditorGUIUtility.labelWidth));
            GUILayout.Label(contentText);
        });
    }

    public static void TextBox (string text) {
        DrawDisabled(() => {
            GUILayout.TextArea(text);
        });
    }

    public static void TextBoxCentered (string text) {
        DrawDisabled(() => {
            DrawCentered(() => {
                GUILayout.TextArea(text);
            });
        });
    }

    public static bool ButtonCentered (string text, float width, bool miniButton = false) {
        var output = false;
        DrawCentered(() => {
            if(miniButton){
                output = GUILayout.Button(text, EditorStyles.miniButton, GUILayout.Width(width));
            }else{
                output = GUILayout.Button(text, GUILayout.Width(width));
            }
        });
        return output;
    }

    public static bool ButtonCenteredWithTint (string text, float width, Color tintColor, float tintStrength = BACKGROUND_TINT_STRENGTH, bool miniButton = false) {
        var output = false;
        DrawWithTintedBackground(() => {output = ButtonCentered(text, width, miniButton);}, tintColor, tintStrength);
        return output;
    }
	
}
