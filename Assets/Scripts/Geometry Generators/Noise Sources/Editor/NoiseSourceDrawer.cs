using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GeometryGenerators {

    [CustomPropertyDrawer(typeof(NoiseSource))]
    public abstract class NoiseSourceDrawer : PropertyDrawer {

        bool showTransform = false;

        // public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        //     return 5f * EditorGUIUtility.singleLineHeight;
        // }

        // public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

        // }

        protected void DrawBase () {
            var stCache = showTransform;
            // showTransform = 
        }
        
    }

}