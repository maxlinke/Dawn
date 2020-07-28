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
        Time.timeScale = 1f;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(FramerateAndTimeScaleSetter))]
public class FrameRateSetterEditor : RuntimeMethodButtonEditor {}
#endif