using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public static class EditorCommands {

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

    [MenuItem("Commands/Start With Bootstrapper %&#p")]
    static void PlayWithBootstrapper () {
        BootstrapSceneLoader.StartPlayMode();
    }
	
}
