using UnityEditor;

namespace GeometryGenerators {

    [CustomEditor(typeof(TerrainGenerator))]
    public class TerrainGeneratorEditor : PlaneGeneratorEditor {

        protected override void DrawOwnProperties () {
            base.DrawOwnProperties();
            var seedProp = serializedObject.FindProperty("seededRandomness");
            EditorGUILayout.PropertyField(seedProp);
            if(seedProp.boolValue){
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("seed"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("noiseOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("noiseStrength"));
            var noiseTypeProp = serializedObject.FindProperty("noiseSourceType");
            EditorGUILayout.PropertyField(noiseTypeProp);
            if(noiseTypeProp.enumValueIndex == (int)(TerrainGenerator.NoiseSourceType.PERLIN)){
                EditorGUILayout.PropertyField(serializedObject.FindProperty("perlinNoiseSources"), true);
            }else if(noiseTypeProp.enumValueIndex == (int)(TerrainGenerator.NoiseSourceType.TEXTURE)){
                EditorGUILayout.PropertyField(serializedObject.FindProperty("textureNoiseSources"), true);
            }
        }

    }

}