using UnityEngine;
using UnityEditor;

namespace GeometryGenerators {

    [CustomPropertyDrawer(typeof(TextureNoiseSource))]
    public class TextureNoiseSourceDrawer : NoiseSourceDrawer {

        protected override int AdditionalPropCount => 1;

        protected override void DrawAdditionalProperty (int index, Rect rect, SerializedProperty property) {
            EditorTools.DrawPropWithManualLabel(rect, 50f, property.FindPropertyRelative("texture"), "TEX");
        }

    }

}