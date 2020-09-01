using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Water/Fog Settings", fileName = "New WaterFog")]
public class WaterFog : ScriptableObject {

    [SerializeField] bool overrideFog = true;
    [SerializeField] FogSettings fogSettings = FogSettings.Default;

    public bool OverrideFog => overrideFog;
    public FogSettings FogSettings => fogSettings;
	
}

#if UNITY_EDITOR

[CustomEditor(typeof(WaterFog))]
public class WaterFogEditor : Editor {

    WaterFog wfs;

    void OnEnable () {
        wfs = target as WaterFog;
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        var guiCache = GUI.enabled;
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(wfs), wfs.GetType(), false);
        EditorGUILayout.Space();
        GUI.enabled = guiCache;

        var overrideProp = serializedObject.FindProperty("overrideFog");
        EditorGUILayout.PropertyField(overrideProp);
        if(!overrideProp.boolValue){
            GUI.enabled = false;
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fogSettings"));
        GUI.enabled = guiCache;

        serializedObject.ApplyModifiedProperties();
    }

}

#endif