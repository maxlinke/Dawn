using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugConsole : MonoBehaviour {

// TODO this needs to be instantiated/initialized even before the inputsystem
// also look into dontdestroyonload for the inputsystem and this (instead of additively loading scenes all the time)
// TODO colorschemes a bit a la cgbasics but abstract 

    [Header("Components")]
    [SerializeField] Text textField = default;
    [SerializeField] ScrollRect scrollView = default;
    [SerializeField] RectTransform scrollViewContent = default;

    // [Header("Settings")]
    // [SerializeField] float debug;
    
    private static string log = string.Empty;

    void Awake () {
        DontDestroyOnLoad(gameObject);
        Clear();
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy () {
        Application.logMessageReceived -= HandleLog;
    }

    // TODO exceptions in bold with stacktrace smaller
    // also time?
    // function to process the log into a string. not in here.
    void HandleLog (string logString, string stackTrace, LogType logType) {
        log += $"\n<color=#{ColorUtility.ToHtmlStringRGB(ColorForLogType(logType))}>{logString}</color>\n";
        if(textField.gameObject.activeSelf){
            var normedPos = scrollView.verticalNormalizedPosition;
            UpdateDisplay();
            if(normedPos <= 0){
                scrollView.verticalNormalizedPosition = 0;
            }
        }
    }

    // TODO show and hide (static)
    // show autoscrolls to bottom

    void UpdateDisplay () {
        textField.text = log;
        scrollViewContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textField.preferredHeight);
    }

    Color ColorForLogType (LogType logType) {
        switch(logType){
            case LogType.Assert: return 0.5f * Color.green + 0.5f * Color.white;
            case LogType.Error: return 0.5f * Color.red + 0.5f * Color.white;
            case LogType.Exception: return 0.5f * Color.red + 0.5f * Color.white;
            case LogType.Log: return Color.white;
            case LogType.Warning: return 0.5f * Color.yellow + 0.5f * Color.white;
            default: return Color.magenta;
        }
    }

    public static void Clear () {
        log = string.Empty;
    }


}
