using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaterFog))]
public class WaterFogEditor : Editor {

    WaterFog wfs;

    void OnEnable () {
        wfs = target as WaterFog;
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        EditorTools.DrawScriptReference(wfs);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("overlayColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("multiplyColor"));

        var overrideProp = serializedObject.FindProperty("overrideFog");
        EditorGUILayout.PropertyField(overrideProp);
        var guiCache = GUI.enabled;
        if(!overrideProp.boolValue){
            GUI.enabled = false;
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fogSettings"));
        GUI.enabled = guiCache;

        serializedObject.ApplyModifiedProperties();
    }

}