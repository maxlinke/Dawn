using UnityEditor;

namespace GeometryGenerators {

    [CustomEditor(typeof(TerrainGenerator))]
    public class TerrainGeneratorEditor : PlaneGeneratorEditor {

        SerializedProperty seededRandomness;
        SerializedProperty clampNoise;
        SerializedProperty noiseType;

        protected override void OnEnable () {
            base.OnEnable();
            seededRandomness = serializedObject.FindProperty("seededRandomness");
            clampNoise = serializedObject.FindProperty("clampNoise");
            noiseType = serializedObject.FindProperty("noiseSourceType");
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            if(base.DrawPropertyCustom(property)){
                return true;
            }
            switch(property.name){
                case "terrainSeed":
                    if(seededRandomness.boolValue) DrawPropIndented();
                    return true;
                case "lowerClamp":
                    if(clampNoise.boolValue) DrawPropIndented();
                    return true;
                case "upperClamp":
                    if(clampNoise.boolValue) DrawPropIndented();
                    return true;
                case "perlinNoiseSources":
                    if(noiseType.enumValueIndex == (int)(TerrainGenerator.NoiseSourceType.Perlin)) DrawProp();
                    return true;
                case "textureNoiseSources":
                    if(noiseType.enumValueIndex == (int)(TerrainGenerator.NoiseSourceType.Texture)) DrawProp();
                    return true;
                default:
                    return false;
            }

            void DrawProp () {
                EditorGUILayout.PropertyField(property, true);
            }

            void DrawPropIndented () {
                EditorTools.DrawIndented(DrawProp);
            }

        }

    }

}