using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(EnforceTypeAttribute))]
public class EnforceTypeAttributeDrawer : PropertyDrawer {

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        if(property.propertyType != SerializedPropertyType.ObjectReference){
            EditorGUI.LabelField(position, $"Invalid property type \"{property.propertyType}\"!");
            return;
        }
        var attributeTargetType = ((EnforceTypeAttribute)attribute).targetType;
        var redIfNull = ((EnforceTypeAttribute)attribute).redIfNull;
        EditorGUI.BeginChangeCheck();
        if(redIfNull){
            RedIfEmptyAttributeDrawer.DrawProperty(position, property, label);
        }else{
            EditorGUI.PropertyField(position, property, label, true);
        }
        if(EditorGUI.EndChangeCheck()){
            var newValue = property.objectReferenceValue;
            if(newValue != null){
                if(newValue is Component newComponent){
                    if(newComponent.TryGetComponent(attributeTargetType, out var actualComponent)){
                        newValue = actualComponent;
                    }else{
                        newValue = null;
                    }
                }else if(newValue is GameObject newGameObject){
                    if(newGameObject.TryGetComponent(attributeTargetType, out var actualComponent)){
                        newValue = actualComponent;
                    }else{
                        newValue = null;
                    }
                }else if(!newValue.GetType().IsInstanceOfType(attributeTargetType)){
                    newValue = null;
                }
            }
            property.objectReferenceValue = newValue;
        }
    }
    
}
