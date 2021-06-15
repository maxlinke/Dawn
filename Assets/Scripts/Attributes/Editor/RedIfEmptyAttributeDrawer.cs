using UnityEngine;
using UnityEditor;
using PropType = UnityEditor.SerializedPropertyType;

[CustomPropertyDrawer(typeof(RedIfEmptyAttribute))]
public class RedIfEmptyAttributeDrawer : PropertyDrawer {

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        DrawProperty(position, property, label);
    }

    public static void DrawProperty (Rect position, SerializedProperty property, GUIContent label) {
        var red = ShouldBeRed(property);
        GUIStack.Color.Push(red ? EditorTools.ComfyRed : GUI.color);
        GUIStack.BackgroundColor.Push(red ? EditorTools.ComfyRed : GUI.backgroundColor);
        EditorGUI.PropertyField(position, property, label, true);
        GUIStack.BackgroundColor.Pop();
        GUIStack.Color.Pop();
    }

    static bool ShouldBeRed (SerializedProperty property) {
        switch(property.propertyType){
            case PropType.ObjectReference:
                return property.objectReferenceValue == null;
            case PropType.String:
                return string.IsNullOrWhiteSpace(property.stringValue);
            default:
                return false;
        }
    }
    
}
