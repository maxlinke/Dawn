using UnityEngine;
using UnityEditor;

namespace PlayerController {

    [CustomPropertyDrawer(typeof(Properties.MovementProperties))]
    public class MovementPropertiesDrawer : PropertyDrawer {

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            var lw = EditorGUIUtility.labelWidth;
            var slh = EditorGUIUtility.singleLineHeight;
            var labelRect = new Rect(position.x, position.y, lw, slh);
            EditorGUI.PrefixLabel(labelRect, label);
            var propRect = new Rect(position.x + lw, position.y, position.width - lw, slh);
            DrawProperty(property.FindPropertyRelative("speed"), 0);
            DrawProperty(property.FindPropertyRelative("accel"), 1);
            DrawProperty(property.FindPropertyRelative("drag"), 2);
            EditorGUI.EndProperty();

            void DrawProperty (SerializedProperty subProp, int index) {
                var ownWidth = propRect.width / 3;
                var ownRect = new Rect(propRect.x + (index * ownWidth), propRect.y, ownWidth, propRect.height);
                var labelWidth = Mathf.Min(40f, 0.5f * ownRect.width);
                var ownLabelRect = new Rect(ownRect.x, ownRect.y, labelWidth, ownRect.height);
                EditorGUI.LabelField(ownLabelRect, subProp.displayName);
                var remainder = ownWidth - labelWidth;
                var remainderRect = new Rect(ownRect.x + labelWidth, ownRect.y, remainder, ownRect.height);
                EditorGUI.PropertyField(remainderRect, subProp, GUIContent.none);
            }
        }

    }

}