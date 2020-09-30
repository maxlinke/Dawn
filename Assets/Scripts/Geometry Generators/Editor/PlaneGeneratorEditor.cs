using UnityEngine;
using UnityEditor;

namespace GeometryGenerators {

    [CustomEditor(typeof(PlaneGenerator))]
    public class PlaneGeneratorEditor : GeometryGeneratorEditor { 

        SerializedProperty tileModeProp;
        SerializedProperty triModeProp;
        SerializedProperty xTilesProp;
        SerializedProperty zTilesProp;

        PlaneGenerator.TileMode tileMode => (PlaneGenerator.TileMode)(tileModeProp.enumValueIndex);
        bool quadMode => (tileModeProp.enumValueIndex == (int)(PlaneGenerator.TileMode.Quads));
        bool randomTriMode => (triModeProp.enumValueIndex == (int)(PlaneGenerator.TriMode.Random));

        int xTiles => xTilesProp.intValue;
        int zTiles => zTilesProp.intValue;

        protected override void OnEnable () {
            base.OnEnable();
            tileModeProp = serializedObject.FindProperty("tileMode");
            triModeProp = serializedObject.FindProperty("triMode");
            xTilesProp = serializedObject.FindProperty("xTiles");
            zTilesProp = serializedObject.FindProperty("zTiles");
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            if(base.DrawPropertyCustom(property)){
                return true;
            }
            switch(property.name){
                case "tileMode":
                    var currentMode = (PlaneGenerator.TileMode)(property.enumValueIndex);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(property);
                    if(EditorGUI.EndChangeCheck()){
                        var newMode = (PlaneGenerator.TileMode)(property.enumValueIndex);
                        if(newMode == PlaneGenerator.TileMode.Quads){
                            xTilesProp.intValue = Mathf.Min(xTilesProp.intValue, PlaneGenerator.MAX_X_TILES_QUADS);
                            zTilesProp.intValue = Mathf.Min(zTilesProp.intValue, PlaneGenerator.MAX_Z_TILES_QUADS);
                        }else{
                            xTilesProp.intValue = Mathf.Min(xTilesProp.intValue, PlaneGenerator.MAX_X_TILES_ISOTRIS);
                            zTilesProp.intValue = Mathf.Min(zTilesProp.intValue, PlaneGenerator.MAX_Z_TILES_ISOTRIS);
                        }
                    }
                    return true;
                case "xTiles":
                    TileSlider(xTiles: true);
                    return true;
                case "zTiles":
                    TileSlider(xTiles: false);
                    var verts = PlaneGenerator.VertexCount(tileMode, xTiles, zTiles);
                    var tris = PlaneGenerator.TriangleCount(tileMode, xTiles, zTiles);
                    var warn = (verts > PlaneGenerator.VERTEX_LIMIT) ? $"(too many! max {PlaneGenerator.VERTEX_LIMIT})" : "";
                    EditorTools.DrawIndented(() => {
                        EditorGUILayout.LabelField($"{verts} vertices {warn}", EditorStyles.miniLabel);
                        EditorGUILayout.LabelField($"{tris} triangles", EditorStyles.miniLabel);
                    });
                    return true;
                case "triMode":
                    if(quadMode) EditorGUILayout.PropertyField(property, true);
                    return true;
                case "triSeed":
                    if(quadMode && randomTriMode) EditorTools.DrawIndented(() => EditorGUILayout.PropertyField(property, true));
                    return true;
                default:
                    return false;
            }

            void TileSlider (bool xTiles) {
                int max;
                if(xTiles){
                    max = (quadMode ? PlaneGenerator.MAX_X_TILES_QUADS : PlaneGenerator.MAX_X_TILES_ISOTRIS);
                }else{
                    max = (quadMode ? PlaneGenerator.MAX_Z_TILES_QUADS : PlaneGenerator.MAX_Z_TILES_ISOTRIS);
                }
                var current = property.intValue;
                EditorGUI.BeginChangeCheck();
                var newVal = EditorGUILayout.IntSlider(new GUIContent(property.displayName), current, 1, max);
                if(EditorGUI.EndChangeCheck()){
                    property.intValue = newVal;
                }
            }
        }
    }

}