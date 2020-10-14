using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class BootstrapSceneLoader {

    // this bootstrapper is editor only, so hardcoding this is fine...
    const string initScenePath = "Assets/Scenes/CoreComponentsBootstrap.unity";

    static BootstrapSceneLoader () {
        EditorApplication.playModeStateChanged += ModeChanged;
    }

    static void ModeChanged (PlayModeStateChange change) {
        switch(change){
            case PlayModeStateChange.ExitingEditMode:
                EditorSceneManager.OpenScene(initScenePath, OpenSceneMode.Additive);
                break;
            case PlayModeStateChange.EnteredEditMode:
                var scene = EditorSceneManager.GetSceneByPath(initScenePath);
                EditorSceneManager.CloseScene(scene, true);
                break;
            default:
                break;
        }
    }

}
