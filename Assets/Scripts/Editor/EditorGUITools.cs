using UnityEngine;
using UnityEditor;

public static class EditorGUITools {

    private static float SLH => EditorGUIUtility.singleLineHeight;
    private static float SVS => EditorGUIUtility.standardVerticalSpacing;

    public static Rect NextLine (ref Rect position, float manualIndent = 0f) {
        position = new Rect(position.x, position.y + SLH + SVS, position.width, position.height - SLH - SVS);
        return new Rect(position.x + manualIndent, position.y, position.width - manualIndent, SLH);
    }

    public static Rect NextLineWithWidth (ref Rect position, float width, float manualIndent = 0f) {
        position = new Rect(position.x, position.y + SLH + SVS, position.width, position.height - SLH - SVS);
        return new Rect(position.x + manualIndent, position.y, Mathf.Min(width, position.width - manualIndent), SLH);
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
