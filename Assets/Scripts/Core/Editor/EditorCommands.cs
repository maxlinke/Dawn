using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Persistence;

public static class EditorCommands {

#region Commands

    [MenuItem("Commands/Clear Selection %#d")]
    static void ClearSelection() {
        Selection.activeObject = null;
    }

    [MenuItem("Commands/Clear Console %#k")]
    static void ClearConsole() {
        try{
            var logEntries = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }catch(Exception e){
            Debug.LogError($"Unity changed how clear console works!\n\n{e}");
        }
    }

    [MenuItem("Commands/Toggle UI Layer %u")]
    static void ToggleUI () {
        if((Tools.visibleLayers & Layer.UI.mask) != 0){
            Tools.visibleLayers = ~Layer.UI.mask;
        }else{
            Tools.visibleLayers = Layer.UI.mask;
        }
    }

#endregion

#region Data

    [MenuItem("Data/Open Config Directory", true)]
    private static bool EditorOpenConfigDir_Validate () => FileHelper.ConfigDirectoryExists;

    [MenuItem("Data/Open Config Directory")]
    private static void EditorOpenConfigDir () => FileHelper.OpenConfigDirectory();

    [MenuItem("Data/Open Save File Directory", true)]
    private static bool EditorOpenSaveFileDir_Validate () => FileHelper.SaveFileDirectoryExists;

    [MenuItem("Data/Open Save File Directory")]
    private static void EditorOpenSaveFileDir () => FileHelper.OpenSaveFileDirectory();

    [MenuItem("Data/Open Persistent Data Directory")]
    public static void EditorOpenDataDir () => FileHelper.OpenDataDirectory();

#endregion
	
}
