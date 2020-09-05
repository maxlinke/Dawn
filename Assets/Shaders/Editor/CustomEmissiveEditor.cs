using UnityEngine;
using UnityEditor;

namespace ShaderEditors {

    public class CustomEmissiveEditor : CustomShaderGUI {

        static MaterialProperty[] emptyProps = new MaterialProperty[0];

        public override void OnGUI (MaterialEditor editor, MaterialProperty[] properties) {
            var targetMat = editor.target as Material;
            
            GUILayout.Label("Emission", EditorStyles.boldLabel);
            DrawIndented(() => {
                var colProp = FindProperty("_Color", properties);
                var texProp = FindProperty("_MainTex", properties);
                editor.TexturePropertySingleLine(new GUIContent(texProp.displayName), texProp, colProp);
                editor.TextureScaleOffsetProperty(texProp);
                GUILayout.Space(4f);
                DrawFullGISelection(editor, targetMat);
            });

            GUILayout.Space(10f);
            DrawRenderingOptionsIfExists(editor, properties);
            DrawDefaultBottomProperties(editor);
        }
        
    }

}