using UnityEngine;
using UnityEditor;

namespace GeometryGenerators {

    [CustomPropertyDrawer(typeof(NoiseSource))]
    public abstract class NoiseSourceDrawer : PropertyDrawer {

        protected const float LABELWIDTH = 75f;
        protected const float LEFT_FRAC_WIDTH = 0.5f;
        protected const float RIGHT_FRAC_WIDTH = 0.5f;

        private Rect position;
        private SerializedProperty property;

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            var propCount = AdditionalPropLines + (property.FindPropertyRelative("useCustomTransform").boolValue ? 5 : 3);
            return (propCount + 1) * EditorGUIUtility.singleLineHeight + propCount * EditorGUIUtility.standardVerticalSpacing;
        }

        protected abstract int AdditionalPropLines { get; }

        protected abstract void DrawAdditionalProperty (int index, SerializedProperty property);

        Rect NextLine () {
            return EditorGUITools.RemoveLine(ref position);
        }

        protected void DoubleProp (string prop1Name, string prop1Label, string prop2Name, string prop2Label) {
            var rect = NextLine();
            EditorGUITools.DrawHalfWidthProp(rect, true,  LEFT_FRAC_WIDTH,  LABELWIDTH, property.FindPropertyRelative(prop1Name), prop1Label);
            if(prop2Name != null){
                EditorGUITools.DrawHalfWidthProp(rect, false, RIGHT_FRAC_WIDTH, LABELWIDTH, property.FindPropertyRelative(prop2Name), prop2Label);
            }
        }

        public override void OnGUI (Rect pos, SerializedProperty prop, GUIContent label) {
            this.position = pos;
            this.property = prop;
            EditorGUI.BeginProperty(position, label, property);
            var lw = EditorGUIUtility.labelWidth;
            var slh = EditorGUIUtility.singleLineHeight;
            var svs = EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PrefixLabel(NextLine(), label, EditorStyles.boldLabel);
            EditorGUITools.RemoveRectFromLeft(ref position, 10f, 0f);
            DoubleProp("strength", "Strength", "randomness", "Random");
            DoubleProp("size", "Size", "valueRange", "Value");
            for(int i=0; i<AdditionalPropLines; i++){
                DrawAdditionalProperty(i, property);
            }
            DrawUseCustomTransformTransform(out var showTransform);
            if(showTransform){
                DoubleProp("position", "Position", "angle", "Rotation");
                DoubleProp("vecSize", "Size", null, null);
            }
            EditorGUI.EndProperty();

            void DrawUseCustomTransformTransform (out bool output) {
                var customTransformProp = property.FindPropertyRelative("useCustomTransform");
                var ctr = NextLine();
                var to = 0;
                var tr = new Rect(ctr.x + to, ctr.y, ctr.width, ctr.height);
                customTransformProp.boolValue = EditorGUI.Toggle(tr, customTransformProp.boolValue);
                var lo = 20f;
                var lr = new Rect(ctr.x + lo, ctr.y, ctr.width - lo, ctr.height);
                EditorGUI.LabelField(lr, "Use Custom Transform");
                output = customTransformProp.boolValue;
            }

        }
        
    }

}