// source: https://answers.unity.com/questions/486694/default-editor-enum-as-flags-.html

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
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
        EditorGUI.BeginChangeCheck();
        var origValue = property.intValue;
        var newValue = EditorGUI.MaskField(position, label, origValue, property.enumDisplayNames);
        if(EditorGUI.EndChangeCheck()){
            property.intValue = newValue;
        }
    }

}

#endif