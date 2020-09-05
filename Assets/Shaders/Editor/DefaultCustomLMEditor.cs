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
                    var specHardProp = FindProperty("_SpecHardness", properties);
                    editor.ShaderProperty(specHardProp, specHardProp.displayName);
                    var sampleSpecProp = FindProperty("_SampleSpecularMap", properties);
                    editor.ShaderProperty(sampleSpecProp, sampleSpecProp.displayName);
                    var specColProp = FindProperty("_SpecColor", properties);
                    if(targetMat.IsKeywordEnabled("_SPECULARMAP")){
                        var specTexProp = FindProperty("_SpecTex", properties);
                        editor.TexturePropertySingleLine(new GUIContent(specTexProp.displayName), specTexProp, specColProp);
                        DrawIndented(() => {editor.TextureScaleOffsetProperty(specTexProp);});
                    }else{
                        editor.ShaderProperty(specColProp, specColProp.displayName);
                    }
                });
            }

            if(targetMat.HasProperty("_BumpMap")){
                GUILayout.Label("Normals", EditorStyles.boldLabel);
                DrawIndented(() => {
                    var normalMapProp = FindProperty("_BumpMap", properties);
                    editor.TexturePropertySingleLine(new GUIContent(normalMapProp.displayName), normalMapProp);
                    DrawIndented(() => {editor.TextureScaleOffsetProperty(normalMapProp);});
                });
            }

            if(targetMat.HasProperty("_Emissive")){
                GUILayout.Label("Emission", EditorStyles.boldLabel);
                DrawIndented(() => {
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
                    if(targetMat.IsKeywordEnabled("_EMISSIVE")){
                        var emitColProp = FindProperty("_EmissionColor", properties);
                        var emitTexProp = FindProperty("_EmissionTex", properties);
                        editor.TexturePropertySingleLine(new GUIContent(emitTexProp.displayName), emitTexProp, emitColProp);
                        DrawIndented(() => {editor.TextureScaleOffsetProperty(emitTexProp);});
                        DrawFullGISelection(editor, targetMat);
                    }
                });
            }

            GUILayout.Space(10f);
            DrawRenderingOptionsIfExists(editor, properties);
            DrawDefaultBottomProperties(editor);
        }
        
    }

}