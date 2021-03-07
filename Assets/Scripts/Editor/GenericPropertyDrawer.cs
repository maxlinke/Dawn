using UnityEngine;
using UnityEditor;

public abstract class GenericPropertyDrawer: PropertyDrawer {

    public enum LabelPosition {
        OnTop,
        OnLeft
    }

    protected SerializedProperty serializedProperty { get; private set; }

    protected virtual float rawLabelWidth => 65;
    protected virtual float labelSpace => 2;

    protected abstract LabelPosition labelPosition { get; }
    protected abstract bool alwaysIgnoreLabel { get; }

    public override float GetPropertyHeight (SerializedProperty inputProperty, GUIContent label) {
        this.serializedProperty = inputProperty.Copy();
        float height = 0;
        if((labelPosition == LabelPosition.OnTop) && (label != GUIContent.none) && !alwaysIgnoreLabel){
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
        foreach(var prop in inputProperty.IterateOverVisibleChildren()){
            if(PropertyIsDrawnCustom(prop, out var customHeight)){
                height += customHeight;
            }else{
                height += EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
            }
            height += EditorGUIUtility.standardVerticalSpacing;
        }
        return height;
    }

    public override void OnGUI (Rect position, SerializedProperty inputProperty, GUIContent label) {
        this.serializedProperty = inputProperty.Copy();
        int localIndent = 0;
        if(label != GUIContent.none && !alwaysIgnoreLabel){
            if(labelPosition == LabelPosition.OnTop){
                var labelRect = EditorGUITools.RemoveLine(ref position);
                EditorGUI.LabelField(labelRect, label);
                localIndent = 1;
            }else{
                var labelWidth = rawLabelWidth - (15 * EditorGUI.indentLevel);
                var labelRect = new Rect(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, label);
                var offset = labelWidth + labelSpace;
                position = new Rect(position.x + offset, position.y, position.width - offset, position.height);
                localIndent = 0;
            }
        }
        EditorGUI.indentLevel += localIndent;
        foreach(var prop in inputProperty.IterateOverVisibleChildren()){
            if(PropertyIsDrawnCustom(prop, out var customHeight)){
                var rect = EditorGUITools.RemoveLine(ref position, customHeight);
                DrawPropertyCustom(rect, prop);
            }else{
                var propLabel = new GUIContent(prop.displayName);
                var height = EditorGUI.GetPropertyHeight(prop, propLabel, true);
                var rect = EditorGUITools.RemoveLine(ref position, height);
                EditorGUI.PropertyField(rect, prop, propLabel, true);
            }
        }
        EditorGUI.indentLevel -= localIndent;
    }

    protected float DefaultHeight (string propName) {
        return DefaultHeight(serializedProperty.FindPropertyRelative(propName));
    }

    protected float DefaultHeight (SerializedProperty property) {
        return EditorGUI.GetPropertyHeight(property);
    }

    protected abstract bool PropertyIsDrawnCustom (SerializedProperty property, out float height);
    protected abstract void DrawPropertyCustom (Rect position, SerializedProperty property);
    
}
