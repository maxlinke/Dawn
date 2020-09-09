using UnityEngine;
using UnityEditor;

namespace ShaderEditors {

    public class DefaultCustomLMEditor : CustomShaderGUI {

        public override void OnGUI (MaterialEditor editor, MaterialProperty[] properties) {
            var targetMat = GetTargetMaterial(editor);
            DrawDiffuseProperties(editor, properties, targetMat);
            if(targetMat.HasProperty("_SpecColor")){
                DrawSpecularProperties(editor, properties, targetMat);
            }
            if(targetMat.HasProperty("_BumpMap")){
                DrawNormalMapProperties(editor, properties);
            }
            if(targetMat.HasProperty("_Emissive")){
                DrawEmissionProperties(editor, properties, targetMat);
            }
            DrawFooterProperties(editor, properties);
        }

        protected Material GetTargetMaterial (MaterialEditor editor) {
            return editor.target as Material;
        }

        protected void DrawDiffuseProperties (MaterialEditor editor, MaterialProperty[] properties, Material targetMat) {
            GUILayout.Label("Diffuse", EditorStyles.boldLabel);
            DrawIndented(() => {
                var mainColProp = FindProperty("_Color", properties);
                var mainTexProp = FindProperty("_MainTex", properties);
                if(targetMat.HasProperty("_AlphaCutoff")){
                    var cutoffProp = FindProperty("_AlphaCutoff", properties);
                    editor.TexturePropertySingleLine(new GUIContent(mainTexProp.displayName), mainTexProp, mainColProp, cutoffProp);
                }else{
                    editor.TexturePropertySingleLine(new GUIContent(mainTexProp.displayName), mainTexProp, mainColProp);
                }
                DrawIndented(() => {editor.TextureScaleOffsetProperty(mainTexProp);});
            });
        }

        protected void DrawSpecularProperties (MaterialEditor editor, MaterialProperty[] properties, Material targetMat) {
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

        protected void DrawNormalMapProperties (MaterialEditor editor, MaterialProperty[] properties) {
            GUILayout.Label("Normals", EditorStyles.boldLabel);
            DrawIndented(() => {
                var normalMapProp = FindProperty("_BumpMap", properties);
                editor.TexturePropertySingleLine(new GUIContent(normalMapProp.displayName), normalMapProp);
                DrawIndented(() => {editor.TextureScaleOffsetProperty(normalMapProp);});
            });
        }

        protected void DrawEmissionProperties (MaterialEditor editor, MaterialProperty[] properties, Material targetMat) {
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

        protected void DrawFooterProperties (MaterialEditor editor, MaterialProperty[] properties, bool addSpace = true) {
            if(addSpace){
                GUILayout.Space(10f);
            }
            DrawRenderingOptionsIfExists(editor, properties);
            DrawDefaultBottomProperties(editor);
        }
        
    }

}