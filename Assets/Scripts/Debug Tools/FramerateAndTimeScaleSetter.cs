using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FramerateAndTimeScaleSetter : MonoBehaviour {

    [Header("Frame Rate")]
    [SerializeField] int targetFrameRate = 60;
    [SerializeField] bool vSync = false;

    [Header("Time Scale")]
    [SerializeField] float targetTimeScale = 1;

    [Header("Lag Simulation")]
    [SerializeField] bool simulateLag = false;
    [SerializeField] int framerateSpread = 10;
    [SerializeField] bool addFreezes = false;
    [SerializeField] int freezeFrameRate = 2;
    [SerializeField] float freezeProbability = 0.01f;

    bool wasSimulatingLag = false;

    void Update () {
        if(simulateLag){
            QualitySettings.vSyncCount = 0;
            if(addFreezes && Random.value < freezeProbability){
                Application.targetFrameRate = freezeFrameRate;
            }else{
                Application.targetFrameRate = targetFrameRate + Random.Range(-framerateSpread, framerateSpread);
            }
        }else if(wasSimulatingLag){
            SetFrameRate();
        }
        wasSimulatingLag = simulateLag;
    }

    public void SetFrameRate () {
        Application.targetFrameRate = Mathf.Max(1, targetFrameRate);
        targetFrameRate = Application.targetFrameRate;
        QualitySettings.vSyncCount = (vSync ? 1 : 0);
    }

    public void Set60FPS () {
        targetFrameRate = 60;
        vSync = false;
        SetFrameRate();
    }

    public void UnlockFrameRate () {
        Application.targetFrameRate = -1;
        targetFrameRate = Application.targetFrameRate;
        QualitySettings.vSyncCount = 0;
    }

    public void SetTimeScale () {
        Time.timeScale = targetTimeScale;
    }

    public void SlowMoTimeScale () {
        targetTimeScale = 0.1f;
        SetTimeScale();
    }

    public void NormalizeTimeScale () {
        targetTimeScale = 1f;
        SetTimeScale();
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(FramerateAndTimeScaleSetter))]
public class FrameRateSetterEditor : Editor {

    const int cacheSize = 10;
    float[] fpsCache = null;
    int cacheIndex = 0;
    FramerateAndTimeScaleSetter setter;

    void OnEnable () {
        setter = target as FramerateAndTimeScaleSetter;
    }

    public override void OnInspectorGUI () {
        DrawDefaultInspector();
        if(!EditorApplication.isPlaying){
            return;
        }
        DrawInfo();
        GUILayout.Space(10f);
        DrawButtons();

        void DrawInfo () {
            var currentFPS = 1f / Time.unscaledDeltaTime;
            if(fpsCache == null || fpsCache.Length != cacheSize){
                fpsCache = new float[cacheSize];
                for(int i=0; i<fpsCache.Length; i++){
                    fpsCache[i] = currentFPS;
                }
                cacheIndex = 0;
            }
            GUILayout.Space(10f);
            fpsCache[cacheIndex] = currentFPS;
            cacheIndex = (cacheIndex + 1) % fpsCache.Length;
            var avgFPS = 0f;
            for(int i=0; i<fpsCache.Length; i++){
                avgFPS += fpsCache[i];
            }
            avgFPS /= fpsCache.Length;
            if(GUILayout.Button("Update Values", EditorStyles.miniButton)){ }
            GUILayout.Label($"{cacheSize} Frame Average FPS: {avgFPS}");
            GUILayout.Label($"Current DeltaTime: {Time.deltaTime}");
            GUILayout.Label($"Current Unscaled DeltaTime: {Time.unscaledDeltaTime}");
        }
        
        void DrawButtons () {
            if(GUILayout.Button("Set Frame Rate")) setter.SetFrameRate();
            if(GUILayout.Button("Set 60Fps")) setter.Set60FPS();
            if(GUILayout.Button("Unlock Frame Rate")) setter.UnlockFrameRate();
            GUILayout.Space(10f);
            if(GUILayout.Button("Set Time Scale")) setter.SetTimeScale();
            if(GUILayout.Button("Normalize Time Scale")) setter.NormalizeTimeScale();
            if(GUILayout.Button("Slow Mo Time Scale")) setter.SlowMoTimeScale();
        }

    }

}
#endif