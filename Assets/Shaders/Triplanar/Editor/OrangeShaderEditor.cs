using UnityEngine;
using UnityEditor;

namespace ShaderEditors {

    public class OrangeShaderEditor : ShaderGUI {

        static MaterialProperty[] emptyProps = new MaterialProperty[0];

        const string kw_bgOrange = "BACKGROUND_ORANGE";
        const string kw_bgGrey = "BACKGROUND_GREY";

        const string kw_gridWorld = "GRIDCOORDS_TRIPLANAR_WORLD";
        const string kw_gridObject = "GRIDCOORDS_TRIPLANAR_OBJECT";

        const string backgroundColorName = "_BackgroundColor";

        const string gridColorName = "_GridTint";
        const string gridTexName = "_GridTex";
        const string gridScaleName = "_GridTexScale";
        const string gridVectorScaleName = "_GridTexVectorScale";

        const string overlayColorName = "_OverlayTint";
        const string overlayTexName = "_OverlayTex";

        const string tintColorName = "_TintColor";
        const string specColorName = "_SpecColor";
        const string specHardnessName = "_SpecHardness";

        enum BackgroundColor {
            ORANGE,
            GREY,
            CUSTOM
        }

        enum GridCoords {
            WORLD,
            OBJECT,
            UV
        }

        enum GridCoordsOverlay {
            WORLD,
            UV
        }

        // don't have to check if all targets are the same version, unity already does that.
        public override void OnGUI (MaterialEditor editor, MaterialProperty[] properties) {
            var targetMat = editor.target as Material;
            var isOverlayVersion = IsOverlayVersion(targetMat);
            MainColorSelection();
            GridProperties();
            if(IsTintedVersion(targetMat)){
                TintProperties();
            }
            if(isOverlayVersion){
                OverlayProperties();
            }
            base.OnGUI(editor, emptyProps);     // renderqueue etc.

            void MainColorSelection () {
                GUILayout.Label("Background", EditorStyles.boldLabel);
                BackgroundColor bgCol;
                bool differentValues = false;
                if(GetKeyword(kw_bgOrange, out var diff1)){
                    bgCol = BackgroundColor.ORANGE;
                    differentValues |= diff1;
                }else if(GetKeyword(kw_bgGrey, out var diff2)){
                    bgCol = BackgroundColor.GREY;
                    differentValues |= diff2;
                }else{
                    bgCol = BackgroundColor.CUSTOM;
                }
                EditorGUI.showMixedValue = differentValues;
                EditorGUI.BeginChangeCheck();
                bgCol = (BackgroundColor)EditorGUILayout.EnumPopup(new GUIContent("Color"), bgCol);
                if(EditorGUI.EndChangeCheck()){
                    ManualRecordUndo("Change Background Color");
                    SetKeyword(kw_bgOrange, bgCol == BackgroundColor.ORANGE);
                    SetKeyword(kw_bgGrey, bgCol == BackgroundColor.GREY);
                }
                if(bgCol == BackgroundColor.CUSTOM){
                    var backgroundColor = FindProperty(backgroundColorName, properties);
                    editor.ShaderProperty(backgroundColor, backgroundColor.displayName);
                }
            }

            void GridProperties () {
                GUILayout.Label("Grid", EditorStyles.boldLabel);
                bool showScaleOffset;
                bool showVectorScale;
                if(!isOverlayVersion){
                    DefaultGridCoords(out showVectorScale, out showScaleOffset);
                }else{
                    OverlayGridCoords(out showScaleOffset);
                    showVectorScale = false;
                }
                var gridTex = FindProperty(gridTexName, properties);
                var gridCol = FindProperty(gridColorName, properties);
                editor.TexturePropertySingleLine(new GUIContent(gridTex.displayName), gridTex, gridCol);
                EditorGUI.indentLevel += 2;
                if(showScaleOffset){
                    editor.TextureScaleOffsetProperty(gridTex);
                }else if(showVectorScale){
                    var gridVectorScale = FindProperty(gridVectorScaleName, properties);
                    editor.ShaderProperty(gridVectorScale, gridVectorScale.displayName);
                }else{
                    var gridScale = FindProperty(gridScaleName, properties);
                    editor.ShaderProperty(gridScale, gridScale.displayName);
                }
                EditorGUI.indentLevel -= 2;

                void DefaultGridCoords (out bool isObject, out bool isUV) {
                    GridCoords gridCoords;
                    var differentValues = false;
                    if(GetKeyword(kw_gridWorld, out var diff1)){
                        gridCoords = GridCoords.WORLD;
                        differentValues |= diff1;
                    }else if(GetKeyword(kw_gridObject, out var diff2)){
                        gridCoords = GridCoords.OBJECT;
                        differentValues |= diff2;
                    }else{
                        gridCoords = GridCoords.UV;
                    }
                    EditorGUI.showMixedValue = differentValues;
                    EditorGUI.BeginChangeCheck();
                    gridCoords = (GridCoords)EditorGUILayout.EnumPopup(new GUIContent("Coordinates"), gridCoords);
                    if(EditorGUI.EndChangeCheck()){
                        ManualRecordUndo("Change Grid Coordinates");
                        SetKeyword(kw_gridObject, gridCoords == GridCoords.OBJECT);
                        SetKeyword(kw_gridWorld, gridCoords == GridCoords.WORLD);
                    }
                    isObject = (gridCoords == GridCoords.OBJECT);
                    isUV = (gridCoords == GridCoords.UV);
                }

                void OverlayGridCoords (out bool isUV) {
                    GridCoordsOverlay gridCoords;
                    var differentValues = false;
                    if(GetKeyword(kw_gridWorld, out differentValues)){
                        gridCoords = GridCoordsOverlay.WORLD;
                    }else{
                        gridCoords = GridCoordsOverlay.UV;
                    }
                    EditorGUI.showMixedValue = differentValues;
                    EditorGUI.BeginChangeCheck();
                    gridCoords = (GridCoordsOverlay)EditorGUILayout.EnumPopup(new GUIContent("Coordinates"), gridCoords);
                    if(EditorGUI.EndChangeCheck()){
                        ManualRecordUndo("Change Grid Coordinates");
                        SetKeyword(kw_gridObject, false);
                        SetKeyword(kw_gridWorld, gridCoords == GridCoordsOverlay.WORLD);
                    }
                    isUV = (gridCoords == GridCoordsOverlay.UV);
                }
            }

            void OverlayProperties () {
                GUILayout.Label("Overlay", EditorStyles.boldLabel);
                var overlayCol = FindProperty(overlayColorName, properties);
                var overlayTex = FindProperty(overlayTexName, properties);
                editor.TexturePropertySingleLine(new GUIContent(overlayTex.displayName), overlayTex, overlayCol);
                EditorGUI.indentLevel += 2;
                editor.TextureScaleOffsetProperty(overlayTex);
                EditorGUI.indentLevel -= 2;
            }

            void TintProperties () {
                GUILayout.Label("Tint and Reflection", EditorStyles.boldLabel);
                var tintCol = FindProperty(tintColorName, properties);
                var specCol = FindProperty(specColorName, properties);
                var specHardness = FindProperty(specHardnessName, properties);
                editor.ShaderProperty(tintCol, tintCol.displayName);
                editor.ShaderProperty(specCol, specCol.displayName);
                editor.ShaderProperty(specHardness, specHardness.displayName);
            }

            bool IsOverlayVersion (Material mat) {
                return mat.HasProperty(overlayTexName);
            }

            bool IsTintedVersion (Material mat) {
                return mat.HasProperty(tintColorName);
            }
            
            bool GetKeyword (string keyword, out bool differentValues) {
                var enabledAtMain = ((Material)(editor.target)).IsKeywordEnabled(keyword);
                differentValues = false;
                foreach(Material other in editor.targets){
                    differentValues |= other.IsKeywordEnabled(keyword) != enabledAtMain;
                }
                return enabledAtMain;
            }

            void SetKeyword (string keyword, bool keywordState) {
                if(keywordState){
                    foreach(Material target in editor.targets){
                        target.EnableKeyword(keyword);
                    }
                }else{
                    foreach(Material target in editor.targets){
                        target.DisableKeyword(keyword);
                    }
                }
            }

            void ManualRecordUndo (string label) {
                editor.RegisterPropertyChangeUndo(label);
            }
        }
        
    }

}