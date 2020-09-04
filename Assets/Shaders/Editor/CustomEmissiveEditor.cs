using UnityEngine;
using UnityEditor;

namespace ShaderEditors {

    public class CustomEmissiveEditor : ShaderGUI {

        static MaterialProperty[] emptyProps = new MaterialProperty[0];

        public override void OnGUI (MaterialEditor editor, MaterialProperty[] properties) {
            var targetMat = editor.target as Material;
            
            var colProp = FindProperty("_Color", properties);
            var texProp = FindProperty("_MainTex", properties);
            editor.TexturePropertySingleLine(new GUIContent(texProp.displayName), texProp, colProp);
            editor.TextureScaleOffsetProperty(texProp);

            EditorGUI.BeginChangeCheck();
            var gi = targetMat.globalIlluminationFlags;
            gi = (MaterialGlobalIlluminationFlags)EditorGUILayout.EnumPopup(new GUIContent("Global Illumination"), gi);
            if(EditorGUI.EndChangeCheck()){
                editor.RegisterPropertyChangeUndo("Global Illumination Flags");
                targetMat.globalIlluminationFlags = gi;
            }
            
            base.OnGUI(editor, emptyProps);     // renderqueue etc.
        }
        
    }

}