using UnityEngine;
using UnityEditor;

namespace SceneLoading {

    [CustomEditor(typeof(SceneLoader))]
    public class SceneLoaderEditor : Editor {

        bool gotScene = false;
        SceneID selectedScene = default;
        LoadMode selectedMode = LoadMode.WithLoadingScreen;

        public override void OnInspectorGUI () {
            DrawDefaultInspector();
            if(!EditorApplication.isPlaying){
                return;
            }
            GUILayout.Space(10f);
            EditorTools.DrawCentered(() => GUILayout.Label("Runtime Scene Loading", EditorStyles.boldLabel));
            GUILayout.Space(10f);
            if(!gotScene){
                selectedScene = SceneLoader.CurrentScene;
                gotScene = true;
            }
            EditorGUITools.ManualEnumPopup(ref selectedScene, "Scene");
            EditorGUITools.ManualEnumPopup(ref selectedMode, "Load Mode");
            GUILayout.Space(10f);
            if(EditorTools.ButtonCentered("Load Scene", 400)){
                SceneLoader.LoadScene(selectedScene, selectedMode);
            }
        }
        
    }

}