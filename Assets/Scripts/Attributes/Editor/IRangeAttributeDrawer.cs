using UnityEngine;
using UnityEditor;

public abstract class IRangeAttributeDrawer : PropertyDrawer {

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
        var lw = EditorGUIUtility.labelWidth;
        var range = attribute as IRangeAttribute;
        switch(property.propertyType){
            case SerializedPropertyType.Float:
                FloatField();
                break;
            case SerializedPropertyType.Integer:
                IntField();
                break;
            default:
                var labelRect = new Rect(position.x,     position.y, lw,                  position.height);
                var propRect = new Rect(position.x + lw, position.y, position.width - lw, position.height);
                var message = $"Use {attribute.GetType().ToString()} with float or int.";
                if(label == null || label == GUIContent.none){
                    EditorGUI.LabelField(position, message);
                }else{
                    EditorGUI.LabelField(labelRect, label, EditorStyles.label);
                    EditorGUI.LabelField(propRect, message);
                }
                break;
        }

        void FloatField () {
            EditorGUI.BeginChangeCheck();
            float origVal = Mathf.Clamp(property.floatValue, range.fMin, range.fMax);
            float newVal;
            if(range.useSlider){
                newVal = EditorGUI.Slider(position, label, origVal, range.fMin, range.fMax);
            }else{
                newVal = EditorGUI.FloatField(position, label, origVal);
            }
            if(EditorGUI.EndChangeCheck()){
                property.floatValue = Mathf.Clamp(newVal, range.fMin, range.fMax);
            }
        }

        void IntField () {
            EditorGUI.BeginChangeCheck();
            int origVal = Mathf.Clamp(property.intValue, range.iMin, range.iMax);
            int newVal;
            if(range.useSlider){
                newVal = EditorGUI.IntSlider(position, label, origVal, range.iMin, range.iMax);
            }else{
                newVal = EditorGUI.IntField(position, label, origVal);    
            }
            if(EditorGUI.EndChangeCheck()){
                property.intValue = Mathf.Clamp(newVal, range.iMin, range.iMax);
            }
        }
    }

}