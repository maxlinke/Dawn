using UnityEngine;
using UnityEditor;
using Type = UnityEditor.SerializedPropertyType;

[CustomPropertyDrawer(typeof(RedIfEmptyAttribute))]
public class RedIfEmptyAttributeDrawer : PropertyDrawer {

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        var red = ShouldBeRed(property);
        GUIStack.Color.Push(red ? EditorTools.ComfyRed : GUI.color);
        GUIStack.BackgroundColor.Push(red ? EditorTools.ComfyRed : GUI.backgroundColor);
        EditorGUI.PropertyField(position, property, label, true);
        GUIStack.BackgroundColor.Pop();
        GUIStack.Color.Pop();
    }

    bool ShouldBeRed (SerializedProperty property) {
        switch(property.propertyType){
            case Type.ObjectReference:
                return property.objectReferenceValue == null;
            case Type.String:
                return string.IsNullOrWhiteSpace(property.stringValue);
            default:
                return false;
        }
    }
    
}
