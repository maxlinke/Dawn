using UnityEngine;

public class GameInitializer : MonoBehaviour {

    [SerializeField, RedIfEmpty] DebugTools.DebugLog m_debugLog;
    [SerializeField, RedIfEmpty] DebugTools.FramerateDisplay m_fpsDisplay;
    [SerializeField, RedIfEmpty] DebugTools.PlayerControllerDebugUI m_playerDebugUI;
    [SerializeField, RedIfEmpty] UnityEngine.EventSystems.EventSystem m_eventSystem;
    [SerializeField] GameObject[] m_dontDestroyOnLoad;

    static bool gameInitialized = false;

    public static void EnsureGameInitialized () {
        if(gameInitialized){
            return;
        }
        // TODO alternative that also works in builds:
        // move this prefab into resources
        // then both can load it
#if UNITY_EDITOR
        var path = UnityEditor.AssetDatabase.GUIDToAssetPath("56255569b7329e0468f9e49be4544043");
        var initializerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameInitializer>(path);
        var initializer = Instantiate(initializerPrefab);
        initializer.InitializeGame();
#else
        // for builds, they should be getting initialized in the boot scene via an explicit call to the object...
        Debug.LogError("The game should already be initialized, something must have gone VERY wrong here!");
#endif
    }

    public void InitializeGame () {
        if(gameInitialized){
            Debug.LogWarning("Game is already initialized! Aborting call...");
            return;
        }
        m_debugLog.Initialize();
        m_debugLog.visible = false;
        m_fpsDisplay.Initialize();
        m_fpsDisplay.SetMode(DebugTools.FramerateDisplay.Mode.Small, updateVisuals: false);
        m_playerDebugUI.Initialize();
        m_playerDebugUI.visible = false;
        m_eventSystem.transform.SetParent(null);
        DontDestroyOnLoad(m_eventSystem);
        CustomInputSystem.InputSystem.EnsureExists();
        foreach(var go in m_dontDestroyOnLoad){
            go.transform.SetParent(null);
            DontDestroyOnLoad(go);
        }
        gameInitialized = true;
    }

}
