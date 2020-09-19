using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(UnitAttribute))]
public class UnitAttributeDrawer : PropertyDrawer {

    const float margin = 4f;

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
        var unit = attribute as UnitAttribute;
        GetRects(position, unit.labelWidth, out var propRect, out var unitRect);
        EditorGUI.PropertyField(propRect, property, label, true);
        EditorGUI.LabelField(unitRect, unit.name);
    }

    public static void GetRects (Rect position, float unitLabelWidth, out Rect propRect, out Rect unitRect) {
        var lw = unitLabelWidth;
        var pw = position.width - lw;
        propRect = new Rect(position.x,       position.y, pw - margin, position.height);
        unitRect = new Rect(position.x + pw, position.y, lw,          position.height);
    }

}
