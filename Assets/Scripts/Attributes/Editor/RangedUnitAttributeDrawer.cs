using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(RangedUnitAttribute))]
public class RangedUnitAttributeDrawer : IRangeAttributeDrawer {

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        var ru = attribute as RangedUnitAttribute;
        UnitAttributeDrawer.GetRects(position, ru.labelWidth, out var propRect, out var unitRect);
        base.OnGUI(propRect, property, label);
        EditorGUI.LabelField(unitRect, ru.name);
    }
	
}
