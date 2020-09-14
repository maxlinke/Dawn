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
            var clampProp = serializedObject.FindProperty("clampNoise");
            EditorGUILayout.PropertyField(clampProp);
            if(clampProp.boolValue){
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lowerClamp"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("upperClamp"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("noiseOffsetPre"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nosieOffsetPost"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("noiseStrength"));
            var noiseTypeProp = serializedObject.FindProperty("noiseSourceType");
            // TODO manual drawing of noise sources
            // TODO manual "show transform" for noise sources (use bool array in here)
            // show transform has to be static?
            // needs an additional "override" bool so as to not fuck everything up. release override at the end here
            // default internally is then its own thing
            EditorGUILayout.PropertyField(noiseTypeProp);
            if(noiseTypeProp.enumValueIndex == (int)(TerrainGenerator.NoiseSourceType.Perlin)){
                EditorGUILayout.PropertyField(serializedObject.FindProperty("perlinNoiseSources"), true);
            }else if(noiseTypeProp.enumValueIndex == (int)(TerrainGenerator.NoiseSourceType.Texture)){
                EditorGUILayout.PropertyField(serializedObject.FindProperty("textureNoiseSources"), true);
            }
        }

    }

}