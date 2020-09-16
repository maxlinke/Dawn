using UnityEngine;
using UnityEditor;

namespace GeometryGenerators {

    [CustomPropertyDrawer(typeof(NoiseSource))]
    public abstract class NoiseSourceDrawer : PropertyDrawer {

        protected const float LABELWIDTH = 54f;

        bool showTransform = false;

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            var propCount = AdditionalPropCount + (showTransform ? 6 : 3);
            return (propCount + 1) * EditorGUIUtility.singleLineHeight + propCount * EditorGUIUtility.standardVerticalSpacing;
        }

        protected abstract int AdditionalPropCount { get; }

        protected abstract void DrawAdditionalProperty (int index, Rect rect, SerializedProperty property);

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            var lw = EditorGUIUtility.labelWidth;
            var slh = EditorGUIUtility.singleLineHeight;
            var svs = EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PrefixLabel(position, label);
            float manualIndent = 10f;
            DrawStrengthRandomnessAndSize();
            for(int i=0; i<AdditionalPropCount; i++){
                DrawAdditionalProperty(i, NextLine(), property);
            }
            DrawShowTransform();
            if(showTransform){
                DrawTransform();
            }
            manualIndent = 0f;
            EditorGUI.EndProperty();

            Rect NextLine () {
                return EditorGUITools.NextLine(ref position, manualIndent);
            }

            void DrawStrengthRandomnessAndSize () {
                var rect1 = NextLine();
                EditorGUITools.DrawHalfWidthProp(rect1, true, 0.58f, LABELWIDTH, property.FindPropertyRelative("strength"), "STR");
                EditorGUITools.DrawHalfWidthProp(rect1, false, 0.4f, LABELWIDTH, property.FindPropertyRelative("randomness"), "RAND");
                var rect2 = NextLine();
                EditorGUITools.DrawHalfWidthProp(rect2, true, 0.58f, LABELWIDTH, property.FindPropertyRelative("size"), "SIZE");
                EditorGUITools.DrawHalfWidthProp(rect2, false, 0.4f, LABELWIDTH, property.FindPropertyRelative("valueRange"), "VAL");
            }

            void DrawShowTransform () {
                var str = NextLine();
                var to = 15f;
                var tr = new Rect(str.x + to, str.y, str.width - to, str.height);
                showTransform = EditorGUI.Toggle(tr, showTransform, EditorStyles.foldout);
                var lo = 30f;
                var lr = new Rect(str.x + lo, str.y, str.width - lo, str.height);
                EditorGUI.LabelField(lr, "Show Transform");
            }

            void DrawTransform() {
                EditorGUITools.DrawPropWithManualLabel(NextLine(), 60f, property.FindPropertyRelative("position"), "POS");
                EditorGUITools.DrawPropWithManualLabel(NextLine(), 60f, property.FindPropertyRelative("angle"), "ROT");
                EditorGUITools.DrawPropWithManualLabel(NextLine(), 60f, property.FindPropertyRelative("vecSize"), "SIZE");
            }

        }
        
    }

}