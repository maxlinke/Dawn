using UnityEngine;
using UnityEditor;

public static class EditorTools {

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

    public static void DrawDisabled (System.Action drawAction) {
        var guiOn = GUI.enabled;
        GUI.enabled = false;
        drawAction();
        GUI.enabled = guiOn;
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
	
}
