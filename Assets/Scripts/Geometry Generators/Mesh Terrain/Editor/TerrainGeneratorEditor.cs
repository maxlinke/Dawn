using UnityEditor;

namespace GeometryGenerators {

    [CustomEditor(typeof(TerrainGenerator))]
    public class TerrainGeneratorEditor : PlaneGeneratorEditor {

        protected override void DrawAdditionalProperties () {
            base.DrawAdditionalProperties();

            var seedProp = serializedObject.FindProperty("useSeed");
            EditorGUILayout.PropertyField(seedProp);
            if(seedProp.boolValue){
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("seed"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("noiseOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("noiseStrength"));
            var nstProp = serializedObject.FindProperty("noiseSourceType");
            EditorGUILayout.PropertyField(nstProp);
            if(nstProp.enumValueIndex == (int)(TerrainGenerator.NoiseSourceType.PERLIN)){
                EditorGUILayout.PropertyField(serializedObject.FindProperty("perlinNoiseSources"), true);
            }else if(nstProp.enumValueIndex == (int)(TerrainGenerator.NoiseSourceType.TEXTURE)){
                EditorGUILayout.PropertyField(serializedObject.FindProperty("textureNoiseSources"), true);
            }
        }

    }

}