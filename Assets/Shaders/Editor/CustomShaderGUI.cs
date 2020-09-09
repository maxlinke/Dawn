using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ShaderEditors {

    public abstract class CustomShaderGUI : ShaderGUI {

        static MaterialProperty[] emptyProperties = new MaterialProperty[0];

        bool expandRenderOps = false;
        bool expandBlendOps = true;
        bool expandStencilOps = true;

        public override void OnGUI (MaterialEditor editor, MaterialProperty[] properties) {
            base.OnGUI(editor, properties);
        }

        protected void DrawIndented (System.Action drawAction, int indent = 1) {
            EditorGUI.indentLevel += indent;
            drawAction();
            EditorGUI.indentLevel -= indent;
        }

        protected void DrawDefaultBottomProperties (MaterialEditor editor) {
            base.OnGUI(editor, emptyProperties);
        }

        protected void DrawFullGISelection (MaterialEditor editor, Material targetMat = null) {
            if(targetMat == null){
                targetMat = editor.target as Material;
            }
            EditorGUI.BeginChangeCheck();
            var gi = targetMat.globalIlluminationFlags;
            gi = (MaterialGlobalIlluminationFlags)EditorGUILayout.EnumPopup(new GUIContent("Global Illumination"), gi);
            if(EditorGUI.EndChangeCheck()){
                editor.RegisterPropertyChangeUndo("Global Illumination Flags");
                targetMat.globalIlluminationFlags = gi;
            }
        }

        protected void DrawRenderingOptionsIfExists (MaterialEditor editor, MaterialProperty[] properties) {
            var props = new List<MaterialProperty>();
            TryAdd("_Cull");
            TryAdd("_ZWrite");
            TryAdd("_ZTest");
            TryAdd("_ColorMask");
            DrawAndResetIfNeeded("Rendering", ref expandRenderOps);
            TryAdd("_BlendSrc");
            TryAdd("_BlendDst");
            DrawAndResetIfNeeded("Blending", ref expandBlendOps);
            TryAdd("_StencilID");
            TryAdd("_StencilReadMask");
            TryAdd("_StencilWriteMask");
            TryAdd("_StencilComp");
            TryAdd("_StencilPass");
            TryAdd("_StencilFail");
            TryAdd("_StencilZFail");
            DrawAndResetIfNeeded("Stencil", ref expandStencilOps);

            void TryAdd (string propName) {
                try{
                    var prop = FindProperty(propName, properties);
                    props.Add(prop);
                }catch{}
            }

            void DrawAndResetIfNeeded (string header, ref bool expand) {
                if(!(props.Count > 0)){
                    return;
                }
                expand = GUILayout.Toggle(expand, header, EditorStyles.foldout);
                if(expand){
                    EditorGUI.indentLevel++;
                    foreach(var prop in props){
                        editor.ShaderProperty(prop, prop.displayName);
                    }
                    EditorGUI.indentLevel--;
                }
                props.Clear();
            }
        }
        
    }

}