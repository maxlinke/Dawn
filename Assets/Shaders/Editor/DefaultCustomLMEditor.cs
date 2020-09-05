using UnityEngine;
using UnityEditor;

namespace ShaderEditors {

    public class DefaultCustomLMEditor : CustomShaderGUI {

        public override void OnGUI (MaterialEditor editor, MaterialProperty[] properties) {
            var targetMat = editor.target as Material;

            GUILayout.Label("Diffuse", EditorStyles.boldLabel);
            DrawIndented(() => {
                var mainColProp = FindProperty("_Color", properties);
                var mainTexProp = FindProperty("_MainTex", properties);
                editor.TexturePropertySingleLine(new GUIContent(mainTexProp.displayName), mainTexProp, mainColProp);
                DrawIndented(() => {editor.TextureScaleOffsetProperty(mainTexProp);});
            });

            if(targetMat.HasProperty("_SpecColor")){
                GUILayout.Label("Specular", EditorStyles.boldLabel);
                DrawIndented(() => {
                    var specColProp = FindProperty("_SpecColor", properties);
                    if(targetMat.HasProperty("_SpecTex")){
                        var specTexProp = FindProperty("_SpecTex", properties);
                        editor.TexturePropertySingleLine(new GUIContent(specTexProp.displayName), specTexProp, specColProp);
                        DrawIndented(() => {editor.TextureScaleOffsetProperty(specTexProp);});
                    }else{
                        editor.ShaderProperty(specColProp, specColProp.displayName);
                    }
                    var specHardProp = FindProperty("_SpecHardness", properties);
                    editor.ShaderProperty(specHardProp, specHardProp.displayName);
                });
            }

            if(targetMat.HasProperty("_Emissive")){
                GUILayout.Space(10f);
                var emissiveProp = FindProperty("_Emissive", properties);
                EditorGUI.BeginChangeCheck();
                editor.ShaderProperty(emissiveProp, emissiveProp.displayName);
                if(EditorGUI.EndChangeCheck()){
                    editor.RegisterPropertyChangeUndo("Global Illumination Flags");
                    if(targetMat.IsKeywordEnabled("_EMISSIVE")){
                        targetMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    }else{
                        targetMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                    }
                }
                GUILayout.Space(-6f);
                if(targetMat.IsKeywordEnabled("_EMISSIVE")){
                    GUILayout.Label("Emission", EditorStyles.boldLabel);
                    var emitColProp = FindProperty("_EmissionColor", properties);
                    var emitTexProp = FindProperty("_EmissionTex", properties);
                    editor.TexturePropertySingleLine(new GUIContent(emitTexProp.displayName), emitTexProp, emitColProp);
                    DrawIndented(() => {editor.TextureScaleOffsetProperty(emitTexProp);});
                    DrawFullGISelection(editor, targetMat);
                }
            }

            GUILayout.Space(10f);
            DrawRenderingOptionsIfExists(editor, properties);
            DrawDefaultBottomProperties(editor);
        }
        
    }

}