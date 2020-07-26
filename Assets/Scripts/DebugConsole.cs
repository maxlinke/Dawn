using UnityEngine;
using UnityEngine.UI;
using CustomInputSystem;

public class DebugConsole : MonoBehaviour {

    [Header("Components")]
    [SerializeField] Canvas canvas = default;
    [SerializeField] Text logTextField = default;
    [SerializeField] Text countDisplayTextField = default;
    [SerializeField] Button clearButton = default;
    [SerializeField] Text clearButtonLabel = default;
    [SerializeField] ScrollRect scrollView = default;
    [SerializeField] Image background = default;

    [Header("UI Element Colors")]
    [SerializeField] Color textColor = Color.black;
    [SerializeField] Color backgroundColor = Color.black;
    [SerializeField] Color midgroundColor = Color.black;
    [SerializeField] Color foregroundColor = Color.black;

    [Header("Log Colors")]
    [SerializeField] Color logColor = Color.black;
    [SerializeField] Color warningColor = Color.black;
    [SerializeField] Color errorColor = Color.black;
    [SerializeField] Color exceptionColor = Color.black;

    bool visible { get { return canvas.gameObject.activeSelf; } set { canvas.gameObject.SetActive(value); } }
    InputSystem.Bind toggleBind => InputSystem.Bind.TOGGLE_DEBUG_CONSOLE;
    
    private static DebugConsole instance;
    string log = string.Empty;
    int totalLogCount = 0;
    int logCount = 0;
    int warningCount = 0;
    int errorCount = 0;

    void Awake () {
        if(instance != null){
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        InitUI();
        Clear();
        visible = false;
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy () {
        Application.logMessageReceived -= HandleLog;
        if(this == instance){
            instance = null;
        }
    }

    void Update () {
        if(toggleBind.GetKeyDown()){
            visible = !visible;
            if(visible){
                UpdateDisplay();
                scrollView.verticalNormalizedPosition = 0;
            }
        }
    }

    void HandleLog (string logString, string stackTrace, LogType logType) {
        if(logType == LogType.Assert){
            return;
        }
        HandleLogType(logType, out var coloredLogType);
        var logAppend = $"({coloredLogType.ToUpper()}) {logString.Trim()}\n";
        if(logType == LogType.Exception){
            logAppend += $"{stackTrace.Trim()}\n";
        }
        log += $"\n{logAppend}";
        totalLogCount++;

        if(visible){
            UpdateDisplay();
        }
    }

    void HandleLogType (LogType logType, out string coloredString) {
        string logLabel = logType.ToString();
        switch(logType){
            case LogType.Log:
                logCount++;
                coloredString = FormatWithColor(logLabel, logColor);
                break;
            case LogType.Warning:
                warningCount++;
                coloredString = FormatWithColor(logLabel, warningColor);
                break;
            case LogType.Error:
                errorCount++;
                coloredString = FormatWithColor(logLabel, errorColor);
                break;
            case LogType.Exception:
                errorCount++;
                coloredString = FormatWithColor(logLabel, exceptionColor);
                break;
            default:
                coloredString = FormatWithColor(logLabel, logColor);
                break;
        }
    }

    string FormatWithColor (string textToFormat, Color colorToUse) {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(colorToUse)}>{textToFormat}</color>";
    }

    void UpdateDisplay () {
        var normedPos = scrollView.verticalNormalizedPosition;
        try{
            logTextField.text = log;    // < eventually runs out of vertices (65000) and starts throwing exceptions, thereby creating new logs and thowing more exceptions. 
        }catch{
            Clear();    // but the exception occurs elsewhere, so this doesn't work.
        }
        scrollView.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, logTextField.preferredHeight);
        if(normedPos <= 0){
            scrollView.verticalNormalizedPosition = 0;
        }
        countDisplayTextField.text = $"Total: {totalLogCount} | Logs: {logCount}, Warnings: {warningCount}, Errors: {errorCount}";
    }

    void Clear () {
        log = string.Empty;
        totalLogCount = 0;
        logCount = 0;
        warningCount = 0;
        errorCount = 0;
        if(visible){
            UpdateDisplay();
        }
    }

    void InitUI () {
        background.color = backgroundColor;
        scrollView.verticalScrollbar.GetComponent<Image>().color = midgroundColor;
        scrollView.verticalScrollbar.handleRect.GetComponent<Image>().color = foregroundColor;
        logTextField.text = string.Empty;
        logTextField.color = textColor;
        clearButton.targetGraphic.color = Color.white;
        clearButton.transition = Selectable.Transition.ColorTint;
        var cbCols = new ColorBlock();
        cbCols.fadeDuration = 0f;
        cbCols.colorMultiplier = 1f;
        cbCols.normalColor = Color.clear;
        cbCols.highlightedColor = foregroundColor;
        cbCols.pressedColor = midgroundColor;
        cbCols.disabledColor = Color.magenta;
        clearButton.colors = cbCols;
        clearButton.onClick.AddListener(() => {Clear();});
        clearButtonLabel.color = textColor;
        countDisplayTextField.text = string.Empty;
        countDisplayTextField.color = textColor;
    }
}
