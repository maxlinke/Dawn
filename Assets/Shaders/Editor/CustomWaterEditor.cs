using UnityEngine;
using UnityEditor;

namespace ShaderEditors {

    public class CustomWaterEditor : DefaultCustomLMEditor {

        public override void OnGUI (MaterialEditor editor, MaterialProperty[] properties) {
            var targetMat = GetTargetMaterial(editor);
            DrawDiffuseProperties(editor, properties, targetMat);
            DrawEmissionProperties(editor, properties, targetMat);
            DrawFlowProperties(editor, properties, targetMat);
            DrawFooterProperties(editor, properties);
        }

        void DrawFlowProperties (MaterialEditor editor, MaterialProperty[] properties, Material targetMat) {
            GUILayout.Label("Flow", EditorStyles.boldLabel);
            DrawIndented(() => {
                var flowTex = FindProperty("_FlowTex", properties);
                var flowSpeed = FindProperty("_FlowSpeed", properties);
                var flowDistortion = FindProperty("_FlowDistortion", properties);
                editor.TexturePropertySingleLine(new GUIContent(flowTex.displayName), flowTex);
                editor.TextureScaleOffsetProperty(flowTex);
                editor.ShaderProperty(flowSpeed, flowSpeed.displayName);
                editor.ShaderProperty(flowDistortion, flowDistortion.displayName);
                var offsetToggle = FindProperty("_OffsetVertices", properties);
                editor.ShaderProperty(offsetToggle, offsetToggle.displayName);
                if(targetMat.IsKeywordEnabled("_OFFSET_VERTICES")){
                    var offsetDir = FindProperty("_VertexOffsetDir", properties);
                    editor.ShaderProperty(offsetDir, offsetDir.displayName);
                }
            });
        }
        
    }

}