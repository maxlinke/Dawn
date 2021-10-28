using UnityEngine;
using UnityEditor;

namespace WorkbenchUtil {

    [CustomPropertyDrawer(typeof(NoLabelAttribute))]
    public class NoLabelAttributeDrawer : PropertyDrawer {

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.PropertyField(position, property, GUIContent.none, true);
        }

    }

}