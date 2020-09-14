using UnityEngine;
using UnityEditor;

namespace GeometryGenerators {

    [CustomPropertyDrawer(typeof(NoiseSource))]
    public abstract class NoiseSourceDrawer : PropertyDrawer {

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
                return EditorTools.NextLine(ref position, manualIndent);
            }

            void DrawStrengthRandomnessAndSize () {
                var srRect = NextLine();
                EditorTools.DrawHalfWidthProp(srRect, true, 0.58f, 50f, property.FindPropertyRelative("strength"), "STR");
                EditorTools.DrawHalfWidthProp(srRect, false, 0.4f, 50f, property.FindPropertyRelative("randomness"), "RNG");
                var sizeRect = NextLine();
                EditorTools.DrawHalfWidthProp(sizeRect, true, 0.58f, 50f, property.FindPropertyRelative("size"), "SIZE");
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
                EditorTools.DrawPropWithManualLabel(NextLine(), 60f, property.FindPropertyRelative("position"), "POS");
                EditorTools.DrawPropWithManualLabel(NextLine(), 60f, property.FindPropertyRelative("angle"), "ROT");
                EditorTools.DrawPropWithManualLabel(NextLine(), 60f, property.FindPropertyRelative("vecSize"), "SIZE");
            }

        }
        
    }

}