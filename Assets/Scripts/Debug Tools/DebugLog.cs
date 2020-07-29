using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CustomInputSystem;

namespace DebugTools {

    public class DebugLog : MonoBehaviour {

        const int UNITY_TEXT_CHAR_LIMIT = (65000 / 4) - 1;

        [Header("Components")]
        [SerializeField] Canvas canvas = default;
        [SerializeField] Text logTextFieldTemplate = default;
        [SerializeField] Text countDisplayTextField = default;
        [SerializeField] Button clearButton = default;
        [SerializeField] Text clearButtonLabel = default;
        [SerializeField] ScrollRect scrollView = default;
        [SerializeField] Image background = default;

        [Header("Colors")]
        [SerializeField] DebugToolColorScheme colorScheme = default;

        bool visible {
            get {
                return canvas.enabled;
            } set {
                if(canvas.enabled != value){
                    canvas.enabled = value;
                    if(value){
                        OnShow();
                    }else{
                        OnHide();
                    }
                }
            }
        }
        
        private static DebugLog instance;

        Queue<string> queuedLogs;
        List<Text> clonedTextFields;
        Text activeLogTextField;

        string currentLog;
        int totalLogCount;
        int logCount;
        int warningCount;
        int errorCount;

        string hexLogColor;
        string hexWarningColor;
        string hexRrrorColor;
        string hexExceptionColor;

        void Awake () {
            if(instance != null){
                Debug.LogError($"Singleton violation, instance of {nameof(DebugLog)} is not null!");
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            canvas.sortingOrder = (int)CanvasSortingOrder.DEBUG_LOG;
            InitUI();
            queuedLogs = new Queue<string>();
            clonedTextFields = new List<Text>();
            activeLogTextField = logTextFieldTemplate;
            hexLogColor = ColorUtility.ToHtmlStringRGB(colorScheme.DebugLogColor);
            hexWarningColor = ColorUtility.ToHtmlStringRGB(colorScheme.DebugWarningColor);
            hexRrrorColor = ColorUtility.ToHtmlStringRGB(colorScheme.DebugErrorColor);
            hexExceptionColor = ColorUtility.ToHtmlStringRGB(colorScheme.DebugExceptionColor);
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

        void OnShow () {
            UpdateDisplay();
            scrollView.verticalNormalizedPosition = 0;
            scrollView.horizontalNormalizedPosition = 0;
        }

        void OnHide () {
            if(EventSystem.current != null){
                if(this.transform.HasInHierarchy(EventSystem.current.currentSelectedGameObject)){
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }

        void Update () {
            if(Bind.TOGGLE_DEBUG_LOG.GetKeyDown()){
                visible = !visible;
            }
        }

        void HandleLog (string logString, string stackTrace, LogType logType) {
            if(logType == LogType.Assert){
                return;
            }
            HandleLogType(out var coloredLogType);
            var logAppend = FormLogAppend();
            UpdateLogs();
            totalLogCount++;
            if(visible){
                UpdateDisplay();
            }

            void HandleLogType (out string coloredString) {
                string logLabel = logType.ToString();
                switch(logType){
                    case LogType.Log:
                        logCount++;
                        coloredString = FormatWithColor(logLabel, hexLogColor);
                        break;
                    case LogType.Warning:
                        warningCount++;
                        coloredString = FormatWithColor(logLabel, hexWarningColor);
                        break;
                    case LogType.Error:
                        errorCount++;
                        coloredString = FormatWithColor(logLabel, hexRrrorColor);
                        break;
                    case LogType.Exception:
                        errorCount++;
                        coloredString = FormatWithColor(logLabel, hexExceptionColor);
                        break;
                    default:
                        coloredString = FormatWithColor(logLabel, hexLogColor);
                        break;
                }
            }

            string FormLogAppend () {
                var output = $"({coloredLogType.ToUpper()}) {logString.Trim()}\n";
                if(logType == LogType.Exception){
                    output += $"{stackTrace.Trim()}\n";
                }
                return output;
            }

            void UpdateLogs () {
                if(currentLog.Length + logAppend.Length >= UNITY_TEXT_CHAR_LIMIT){
                    queuedLogs.Enqueue(currentLog);
                    currentLog = logAppend;     // no newline at start because that would result in a two-newline gap between the texts
                }else{
                    currentLog += $"\n{logAppend}";
                }
            }
        }

        string FormatWithColor (object textToFormat, string hexColorToUse) {
            return $"<color=#{hexColorToUse}>{textToFormat}</color>";
        }

        void UpdateDisplay () {
            var normedPos = scrollView.verticalNormalizedPosition;
            while(queuedLogs.Count > 0){
                UpdateCurrentTextField(queuedLogs.Dequeue());
                CreateNewActiveTextField();
            }
            UpdateCurrentTextField(currentLog);
            UpdateCountTextField();
            if(normedPos <= 0){
                scrollView.verticalNormalizedPosition = 0;
            }

            void UpdateCurrentTextField (string textToUpdateWith) {
                activeLogTextField.text = textToUpdateWith;
                var preferredHeight = activeLogTextField.preferredHeight;
                var preferredWidth = activeLogTextField.preferredWidth;
                activeLogTextField.rectTransform.sizeDelta = new Vector2(preferredWidth, preferredHeight);
                scrollView.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(preferredWidth, scrollView.content.rect.width));
                var textPos = activeLogTextField.rectTransform.anchoredPosition.y;
                var lowerTextBorder = textPos - preferredHeight;
                scrollView.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(-lowerTextBorder, scrollView.viewport.rect.height));
            }

            void CreateNewActiveTextField () {
                var newActiveField = Instantiate(activeLogTextField, activeLogTextField.rectTransform.parent);
                newActiveField.text = string.Empty;
                newActiveField.rectTransform.SetAnchoredPositionY(activeLogTextField.rectTransform.anchoredPosition.y - activeLogTextField.rectTransform.rect.height);
                newActiveField.gameObject.name = $"{logTextFieldTemplate.gameObject.name} (Clone {clonedTextFields.Count+1})";
                clonedTextFields.Add(newActiveField);
                activeLogTextField = newActiveField;
            }

            void UpdateCountTextField () {  
                var total = $"Total: {totalLogCount}";
                var logs = $"Logs: {ColorIfGreaterZero(logCount, hexLogColor)}";
                var warnings = $"Warnings: {ColorIfGreaterZero(warningCount, hexWarningColor)}";
                var errors = $"Errors: {ColorIfGreaterZero(errorCount, hexRrrorColor)}";
                countDisplayTextField.text = $"{total} | {logs}, {warnings}, {errors}";
            }

            string ColorIfGreaterZero (int inputCount, string hexColor) {
                return inputCount > 0 ? FormatWithColor(inputCount, hexColor) : inputCount.ToString();
            }
        }

        void Clear () {
            ClearLogs();
            ResetCounts();
            ResetUI();
            if(visible){
                UpdateDisplay();
            }

            void ClearLogs () {
                queuedLogs.Clear();
                currentLog = string.Empty;
            }

            void ResetCounts () {
                totalLogCount = 0;
                logCount = 0;
                warningCount = 0;
                errorCount = 0;
            }

            void ResetUI () {
                foreach(var clone in clonedTextFields){
                    Destroy(clone.gameObject);
                }
                clonedTextFields.Clear();
                activeLogTextField = logTextFieldTemplate;
                activeLogTextField.rectTransform.anchoredPosition = Vector2.zero;
                activeLogTextField.rectTransform.sizeDelta = Vector2.zero;
                scrollView.content.sizeDelta = Vector2.zero;
            }
        }

        void InitUI () {
            background.color = colorScheme.BackgroundColor;
            scrollView.verticalScrollbar.GetComponent<Image>().color = colorScheme.MidgroundColor;
            scrollView.verticalScrollbar.handleRect.GetComponent<Image>().color = colorScheme.ForegroundColor;
            scrollView.horizontalScrollbar.GetComponent<Image>().color = colorScheme.MidgroundColor;
            scrollView.horizontalScrollbar.handleRect.GetComponent<Image>().color = colorScheme.ForegroundColor;
            scrollView.content.SetAnchorAndPivot(0, 1);
            logTextFieldTemplate.rectTransform.SetAnchorAndPivot(0, 1);
            logTextFieldTemplate.text = string.Empty;
            logTextFieldTemplate.color = colorScheme.TextColor;
            clearButton.targetGraphic.color = Color.white;
            clearButton.transition = Selectable.Transition.ColorTint;
            var cbCols = new ColorBlock();
            cbCols.fadeDuration = 0f;
            cbCols.colorMultiplier = 1f;
            cbCols.normalColor = Color.clear;
            cbCols.highlightedColor = colorScheme.ForegroundColor;
            cbCols.pressedColor = colorScheme.MidgroundColor;
            cbCols.disabledColor = Color.magenta;
            clearButton.colors = cbCols;
            clearButton.onClick.AddListener(() => {Clear();});
            clearButtonLabel.color = colorScheme.TextColor;
            countDisplayTextField.text = string.Empty;
            countDisplayTextField.color = colorScheme.TextColor;
        }
    }
}