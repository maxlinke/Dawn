using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class BootstrapSceneLoader {

    // this bootstrapper is editor only, so hardcoding this is fine...
    const string initScenePath = "Assets/Scenes/CoreComponentsBootstrap.unity";

    static BootstrapSceneLoader () {
        EditorApplication.playModeStateChanged += ModeChanged;
    }

    // moving the scene to the beginning is a nice try, but it doesn't enfore its awakes to run first
    [MenuItem("Play/Start With Bootstrapper")]
    static void StartPlayMode () {
        if(!EditorApplication.isPlaying){
            var bootstrapper = EditorSceneManager.OpenScene(initScenePath, OpenSceneMode.Additive);
            if(EditorSceneManager.sceneCount > 1){
                var firstScene = EditorSceneManager.GetSceneAt(0);
                EditorSceneManager.MoveSceneBefore(bootstrapper, firstScene);
            }
            EditorApplication.isPlaying = true;
        }
    }

    static void ModeChanged (PlayModeStateChange change) {
        switch(change){
            case PlayModeStateChange.EnteredEditMode:
                var scene = EditorSceneManager.GetSceneByPath(initScenePath);
                EditorSceneManager.CloseScene(scene, true);
                break;
            default:
                break;
        }
    }

}
