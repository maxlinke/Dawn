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

    [RuntimeMethodButton]
    public void SetFrameRate () {
        Application.targetFrameRate = Mathf.Max(1, targetFrameRate);
        targetFrameRate = Application.targetFrameRate;
        QualitySettings.vSyncCount = (vSync ? 1 : 0);
    }

    [RuntimeMethodButton]
    public void UnlockFrameRate () {
        Application.targetFrameRate = -1;
        targetFrameRate = Application.targetFrameRate;
        QualitySettings.vSyncCount = 0;
    }

    [RuntimeMethodButton]
    public void SetTimeScale () {
        Time.timeScale = targetTimeScale;
    }

    [RuntimeMethodButton]
    public void NormalizeTimeScale () {
        targetTimeScale = 1f;
        Time.timeScale = targetTimeScale;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(FramerateAndTimeScaleSetter))]
public class FrameRateSetterEditor : RuntimeMethodButtonEditor {}
#endif