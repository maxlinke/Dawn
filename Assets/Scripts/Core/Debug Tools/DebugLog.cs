using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CustomInputSystem;

namespace DebugTools {

    public class DebugLog : MonoBehaviour, ICoreComponent {

        [Header("Settings")]
        [SerializeField] bool selfInit = false;

        [Header("Components")]
        [SerializeField] Canvas canvas = default;
        [SerializeField] ScrollingTextDisplay textDisplay = default;
        [SerializeField] Text logTextFieldTemplate = default;
        [SerializeField] Text countDisplayTextField = default;
        [SerializeField] Button clearButton = default;
        [SerializeField] Text clearButtonLabel = default;
        [SerializeField] Image background = default;

        [Header("Colors")]
        [SerializeField] DebugToolColorScheme colorScheme = default;

        [Header("Other Settings")]
        [SerializeField] bool openOnLog = false;
        [SerializeField] bool openOnWarning = false;
        [SerializeField] bool openOnError = false;
        [SerializeField] bool openOnException = false;

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

        int totalLogCount;
        int logCount;
        int warningCount;
        int errorCount;
        int exceptionCount;
        int otherCount;

        string hexLogColor;
        string hexWarningColor;
        string hexErrorColor;
        string hexExceptionColor;
        string hexOtherColor;

        void Awake () {
            if(selfInit){
                InitializeCoreComponent(null);
            }
        }

        public void InitializeCoreComponent (IEnumerable<ICoreComponent> others) {
            if(instance != null){
                Debug.LogError($"Singleton violation, instance of {nameof(DebugLog)} is not null!");
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            canvas.sortingOrder = CanvasSortingOrder.DebugLog;
            InitUI();
            hexLogColor = ColorUtility.ToHtmlStringRGB(colorScheme.DebugLogColor);
            hexWarningColor = ColorUtility.ToHtmlStringRGB(colorScheme.DebugWarningColor);
            hexErrorColor = ColorUtility.ToHtmlStringRGB(colorScheme.DebugErrorColor);
            hexExceptionColor = ColorUtility.ToHtmlStringRGB(colorScheme.DebugExceptionColor);
            hexOtherColor = ColorUtility.ToHtmlStringRGB(colorScheme.DebugOtherColor);
            textDisplay.EnsureInitialized();
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
            textDisplay.gameObject.SetActive(true);
            UpdateCountTextField();
        }

        void OnHide () {
            textDisplay.gameObject.SetActive(false);
            if(EventSystem.current != null){
                if(this.transform.HasInHierarchy(EventSystem.current.currentSelectedGameObject)){
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }

        void Update () {
            if(Bind.TOGGLE_DEBUG_LOG.GetKeyDown()){
                if(!visible){
                    visible = true;
                }else if(CursorLockManager.CursorIsLocked()){
                    CursorLockManager.AddUnlocker(this);
                }else{
                    if(CursorLockManager.IsUnlocker(this)){
                        CursorLockManager.RemoveUnlocker(this);
                    }
                    visible = false;
                }
            }
        }

        void HandleLog (string logString, string stackTrace, LogType logType) {
            if(logType == LogType.Assert){
                return;
            }
            HandleLogType(out var coloredLogType);
            var logAppend = FormLogAppend();
            textDisplay.AppendLine(logAppend);
            totalLogCount++;
            if(visible){
                UpdateCountTextField();
            }

            void HandleLogType (out string coloredString) {
                string logLabel = logType.ToString();
                switch(logType){
                    case LogType.Log:
                        logCount++;
                        coloredString = FormatWithColor(logLabel, hexLogColor);
                        visible |= openOnLog;
                        break;
                    case LogType.Warning:
                        warningCount++;
                        coloredString = FormatWithColor(logLabel, hexWarningColor);
                        visible |= openOnWarning;
                        break;
                    case LogType.Error:
                        errorCount++;
                        coloredString = FormatWithColor(logLabel, hexErrorColor);
                        visible |= openOnError;
                        break;
                    case LogType.Exception:
                        exceptionCount++;
                        coloredString = FormatWithColor(logLabel, hexExceptionColor);
                        visible |= openOnException;
                        break;
                    default:
                        otherCount++;
                        coloredString = FormatWithColor(logLabel, hexOtherColor);
                        visible = true;
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
        }

        string FormatWithColor (object textToFormat, string hexColorToUse) {
            return $"<color=#{hexColorToUse}>{textToFormat}</color>";
        }

        void UpdateCountTextField () {
            var total = $"Total: {totalLogCount}";
            if(totalLogCount <= 0){
                countDisplayTextField.text = total;
                return;
            }
            bool commaNeeded = false;
            var categorized = string.Empty;
            AddCategoryIfCountGreaterZero("Log", logCount, hexLogColor);
            AddCategoryIfCountGreaterZero("Warning", warningCount, hexWarningColor);
            AddCategoryIfCountGreaterZero("Error", errorCount, hexErrorColor);
            AddCategoryIfCountGreaterZero("Exception", exceptionCount, hexExceptionColor);
            AddCategoryIfCountGreaterZero("Other", otherCount, hexOtherColor);
            countDisplayTextField.text = $"{total} | {categorized}";

            void AddCategoryIfCountGreaterZero (string type, int count, string hexColor) {
                if(count > 0){
                    categorized = $"{categorized}{(commaNeeded ? ", " : "")}{FormatWithColor($"{count} {(count > 1 ? $"{type}s" : type)}", hexColor)}";
                    commaNeeded = true;
                }
            }
        }

        void Clear () {
            textDisplay.Clear();
            ResetCounts();
            if(visible){
                UpdateCountTextField();
            }

            void ResetCounts () {
                totalLogCount = 0;
                logCount = 0;
                warningCount = 0;
                errorCount = 0;
                exceptionCount = 0;
                otherCount = 0;
            }
        }

        void InitUI () {
            background.color = colorScheme.BackgroundColor;
            textDisplay.ScrollView.verticalScrollbar.GetComponent<Image>().color = colorScheme.MidgroundColor;
            textDisplay.ScrollView.verticalScrollbar.handleRect.GetComponent<Image>().color = colorScheme.ForegroundColor;
            textDisplay.ScrollView.horizontalScrollbar.GetComponent<Image>().color = colorScheme.MidgroundColor;
            textDisplay.ScrollView.horizontalScrollbar.handleRect.GetComponent<Image>().color = colorScheme.ForegroundColor;
            textDisplay.ScrollView.content.SetAnchorAndPivot(0, 1);
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
            clearButton.onClick.AddListener(() => Clear());
            clearButtonLabel.color = colorScheme.TextColor;
            countDisplayTextField.text = string.Empty;
            countDisplayTextField.color = colorScheme.TextColor;
        }
    }
}