﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RTAxis = UnityEngine.RectTransform.Axis;

public class ScrollingTextDisplay : MonoBehaviour {

    const int UNITY_TEXT_CHAR_LIMIT = (65000 / 4) - 1;

    // copy paste before cut paste for now
    // options
    // - new text gameobject per entry
    // - text objects until they're full
    // - stick to width or expand

    [Header("Components")]
    [SerializeField] Text textFieldTemplate = default;
    [SerializeField] ScrollRect scrollView = default;

    [Header("General Settings")]
    // [SerializeField] bool limitTextWidth = default;      // scroll view sets width according to text. if it's set to wrap, then it'll wrap
    [SerializeField] TextCreation textCreation = default;
    [SerializeField] ScrollOnUpdate updateScroll = default;

    [Header("Visual Settings")]
    [SerializeField] float textMarginLeft = default;
    [SerializeField] float textMarginRight = default;
    [SerializeField] float textMarginTop = default;
    [SerializeField] float textMarginBottom = default;

    bool visible => this.gameObject.activeInHierarchy;
    bool initialized => (activeTextField != null);   // since it's set in the initialization...

    List<string> queuedLines;
    List<Text> clonedTextFields;
    RectTransform scrollViewContent;
    RectTransform verticalScrollbar;
    RectTransform horizontalScrollbar;

    RectTransform _activeRT;
    RectTransform activeRT => _activeRT;

    Text _activeTextField;
    Text activeTextField {
        get => _activeTextField;
        set {
            _activeTextField = value;
            _activeRT = value.rectTransform;
        }
    }


    public enum TextCreation {
        AlwaysCreateNew,
        OnlyCreateNewWhenNecessary
    }

    public enum ScrollOnUpdate {
        AlwaysScrollToBottom,
        OnlyScrollToBottomIfAlreadyAtBottom,
        NeverScrollToBottom
    }

    void OnEnable () {
        EnsureInitialized();
        UpdateDisplay();
        ResetScroll();
    }

    void OnDisable () {
        if(EventSystem.current != null){
            if(this.transform.HasInHierarchy(EventSystem.current.currentSelectedGameObject)){
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    public void EnsureInitialized () {
        if(initialized){
            return;
        }
        if(!visible){
            Debug.LogWarning($"Initializing {nameof(ScrollingTextDisplay)} while it's not visible might not work perfectly well...");
        }
        clonedTextFields = new List<Text>();
        queuedLines = new List<string>();
        activeTextField = textFieldTemplate;
        scrollViewContent = scrollView.content;
        verticalScrollbar = scrollView.verticalScrollbar.transform as RectTransform;
        horizontalScrollbar = scrollView.horizontalScrollbar.transform as RectTransform;
        Clear();
    }

    public void AppendLine (string text) {
        EnsureInitialized();
        queuedLines.Add(text);
        Debug.Log(text);
        if(visible){
            UpdateDisplay();
        }
    }

    void UpdateDisplay () {
        bool wasAtBottom = ScrollViewIsAtBottom;
        DisplayQueuedMessages();
        UpdateScrollPosition();

        void UpdateScrollPosition () {
            switch(updateScroll){
                case ScrollOnUpdate.AlwaysScrollToBottom:
                    ResetScroll();
                    return;
                case ScrollOnUpdate.OnlyScrollToBottomIfAlreadyAtBottom:
                    if(wasAtBottom) ResetScroll();
                    return;
                case ScrollOnUpdate.NeverScrollToBottom:
                    return;
                default:
                    Debug.LogError($"Unknown {nameof(ScrollOnUpdate)} \"{updateScroll}\"!");
                    return;
            }
        }
    }

    bool ScrollViewIsAtBottom => scrollView.verticalNormalizedPosition <= 0;
    void ResetScroll () { 
        scrollView.verticalNormalizedPosition = 0; 
        scrollView.horizontalNormalizedPosition = 0;
    }

    void DisplayQueuedMessages () {
        switch(textCreation){
            case TextCreation.AlwaysCreateNew:
                DisplayQueuedMessagesIndividually();
                break;
            case TextCreation.OnlyCreateNewWhenNecessary:
                DisplayQueuedMessagesInAsFewTextsAsPossible();
                break;
            default:
                Debug.LogError($"Unknown {nameof(TextCreation)} \"{textCreation}\"!");
                return;
        }
        queuedLines.Clear();
    }

    void DisplayQueuedMessagesIndividually () {
        for(int i=0; i<queuedLines.Count; i++){
            UpdateActiveTextField(queuedLines[i]);
            CreateNewActiveTextField();
        }
    }

    void DisplayQueuedMessagesInAsFewTextsAsPossible () {
        var currentText = activeTextField.text;
        for(int i=0; i<queuedLines.Count; i++){
            var newText = queuedLines[i];
            var combinedLength = currentText.Length + newText.Length;
            if(combinedLength >= UNITY_TEXT_CHAR_LIMIT){
                UpdateActiveTextField(currentText);
                CreateNewActiveTextField();
                currentText = string.Empty;
            }else{
                currentText += (currentText.Length > 0 ? $"\n{newText}" : newText);
            }
        }
        UpdateActiveTextField(currentText);
    }

    // TODO ensure that the text field TEMPLATE has a point pivot and anchor...
    void UpdateActiveTextField (string newText) {
        activeTextField.text = newText;
        var preferredHeight = activeTextField.preferredHeight;
        var preferredWidth = activeTextField.preferredWidth;
        activeTextField.rectTransform.sizeDelta = new Vector2(preferredWidth, preferredHeight);
        var contentRect = scrollViewContent.rect;
        var rawScrollWidth = preferredWidth + textMarginLeft + textMarginRight;
        // TODO check if i need the scrollbar width (or what the "spacing" does)
        scrollView.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(rawScrollWidth, scrollView.content.rect.width));
        var textPos = activeTextField.rectTransform.anchoredPosition.y;
        var lowerTextBorder = textPos - preferredHeight;
        var rawScrollHeight = -lowerTextBorder + textMarginBottom;
        scrollView.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(rawScrollHeight, scrollView.viewport.rect.height)); // TODO << why viewport?
    }

    void CreateNewActiveTextField () {
        var newActiveField = Instantiate(activeTextField, activeTextField.rectTransform.parent);
        newActiveField.text = string.Empty;
        newActiveField.rectTransform.SetAnchoredPositionY(activeRT.anchoredPosition.y - activeRT.rect.height);
        newActiveField.gameObject.name = $"{textFieldTemplate.gameObject.name} (Clone {clonedTextFields.Count+1})";
        clonedTextFields.Add(newActiveField);
        activeTextField = newActiveField;
    }

    public void Clear () {
        if(!initialized){
            Debug.LogError($"{nameof(ScrollingTextDisplay)} isn't initialized yet! Aborting...");
            return;
        }
        queuedLines.Clear();
        RemoveClonedTexts();
        activeTextField = textFieldTemplate;
        activeTextField.text = string.Empty;
        activeRT.SetAnchorAndPivot(new Vector2(0, 1));
        activeRT.anchoredPosition = new Vector2(textMarginLeft, -textMarginTop);
        activeRT.sizeDelta = Vector2.zero;
        scrollView.content.sizeDelta = Vector2.zero;

        void RemoveClonedTexts () {
            for(int i=0; i<clonedTextFields.Count; i++){
                Destroy(clonedTextFields[i].gameObject);
            }
            clonedTextFields.Clear();
        }
    }

	
}
