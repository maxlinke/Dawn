using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnumFlagsAttribute : PropertyAttribute {

    public EnumFlagsAttribute () {}
	
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer {

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumDisplayNames);
    }

}

#endif