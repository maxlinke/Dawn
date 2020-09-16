﻿using UnityEngine;
using UnityEditor;

namespace GeometryGenerators {

    [CustomPropertyDrawer(typeof(TextureNoiseSource))]
    public class TextureNoiseSourceDrawer : NoiseSourceDrawer {

        protected override int AdditionalPropCount => 1;

        protected override void DrawAdditionalProperty (int index, Rect rect, SerializedProperty property) {
            EditorGUITools.DrawHalfWidthProp(rect, true, 0.58f, LABELWIDTH, property.FindPropertyRelative("texture"), "TEX");
            EditorGUITools.DrawHalfWidthProp(rect, false, 0.4f, LABELWIDTH, property.FindPropertyRelative("filterSize"), "FTLR");
        }

    }

}