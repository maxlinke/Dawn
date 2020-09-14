using UnityEngine;
using UnityEditor;

namespace GeometryGenerators {

    [CustomPropertyDrawer(typeof(PerlinNoiseSource))]
    public class PerlinNoiseSourceDrawer : NoiseSourceDrawer {

        protected override int AdditionalPropCount => 0;
        protected override void DrawAdditionalProperty (int index, Rect rect, SerializedProperty property) { }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            base.OnGUI(position, property, label);
        }
        
    }

}