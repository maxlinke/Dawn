using UnityEngine;
using UnityEditor;

public static class EditorGUITools {

    private static float SLH => EditorGUIUtility.singleLineHeight;
    private static float SVS => EditorGUIUtility.standardVerticalSpacing;

    public static Rect RemoveLine (ref Rect position) {
        return RemoveLines(ref position, 1, SLH);
    }

    public static Rect RemoveLine (ref Rect position, float lineHeight) {
        return RemoveLines(ref position, 1, lineHeight);
    }

    public static Rect RemoveLines (ref Rect position, int count) {
        return RemoveLines(ref position, count, SLH);
    }

    public static Rect RemoveLines (ref Rect position, int count, float lineHeight) {
        var height = (count * lineHeight) + ((count - 1) * SVS);
        var output = new Rect(position.x, position.y, position.width, height);
        position = new Rect(position.x, position.y + height + SVS, position.width, position.height - height - SVS);
        return output;
    }

    public static Rect RemoveRectFromLeft (ref Rect position, float width, float space = 2) {
        var output = new Rect(
            x: position.x,
            y: position.y,
            width: width,
            height: position.height
        );
        position = new Rect(
            x: position.x + width + space,
            y: position.y,
            width: position.width - width - space,
            height: position.height
        );
        return output;
    }

    public static Rect RemoveRectFromRight (ref Rect position, float width, float space = 2) {
        position = new Rect(
            x: position.x,
            y: position.y,
            width: position.width - width - space,
            height: position.height
        );
        return new Rect(
            x: position.x + position.width + space,
            y: position.y,
            width: width,
            height: position.height
        );
    }

    public static void DrawPropWithManualLabel (Rect baseRect, float labelWidth, SerializedProperty propToDraw, string labelText) {
        var labelRect = new Rect(baseRect.x, baseRect.y, labelWidth, baseRect.height);
        EditorGUI.LabelField(labelRect, labelText);
        var propRect = new Rect(baseRect.x + labelWidth, baseRect.y, baseRect.width - labelWidth, baseRect.height);
        EditorGUI.PropertyField(propRect, propToDraw, GUIContent.none, true);
    }

    public static void DrawHalfWidthProp (Rect baseRect, bool leftSide, float fracWidth, float labelWidth, SerializedProperty propToDraw, string labelText) {
        var halfRect = new Rect(baseRect.x + (leftSide ? 0f : (baseRect.width * (1f - fracWidth))), baseRect.y, baseRect.width * fracWidth, baseRect.height);
        var labelRect = new Rect(halfRect.x, halfRect.y, labelWidth, halfRect.height);
        var propRect = new Rect(halfRect.x + labelWidth, halfRect.y, halfRect.width - labelWidth, halfRect.height);
        EditorGUI.LabelField(labelRect, labelText);
        EditorGUI.PropertyField(propRect, propToDraw, GUIContent.none, true);
    }

    public static void LabelWithoutIndent (Rect labelRect, string labelText) {
        var il = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        EditorGUI.LabelField(labelRect, labelText);
        EditorGUI.indentLevel = il;
    }

    public static void ManualEnumPopup<T> (ref T currentValue, string label) where T : System.Enum {
        EditorGUI.BeginChangeCheck();
        T newSelection = (T)(EditorGUILayout.EnumPopup(label, currentValue));
        if(EditorGUI.EndChangeCheck()){
            currentValue = newSelection;
        }
    }
	
}
