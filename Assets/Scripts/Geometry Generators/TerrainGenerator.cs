using UnityEngine;

namespace GeometryGenerators {

    public class TerrainGenerator : PlaneGenerator {

        private const int MAX_RNG_OFFSET = 1024;

        public enum NoiseSourceType {
            None,
            Perlin,
            Texture
        }

        public enum LowerClampValue {
            None,
            MinusOne,
            Zero
        }

        public enum UpperClampValue {
            None,
            One,
            Zero
        }

        [Header("Deformation Settings")]
        [SerializeField]                 bool seededRandomness = false;
        [SerializeField]                 string terrainSeed = string.Empty;
        [SerializeField]                 bool clampNoise = false;
        [SerializeField]                 LowerClampValue lowerClamp = default;
        [SerializeField]                 UpperClampValue upperClamp = default;
        [SerializeField, Range(-1f, 1f)] float noiseOffsetPre = 0f;
        [SerializeField, Range(-1f, 1f)] float nosieOffsetPost = 0f;
        [SerializeField]                 float noiseStrength = 1f;
        [SerializeField]                 NoiseSourceType noiseSourceType = NoiseSourceType.None;
        [SerializeField]                 PerlinNoiseSource[] perlinNoiseSources = default;
        [SerializeField]                 TextureNoiseSource[] textureNoiseSources = default;

        NoiseSource[] noiseSources;
        System.Func<float, float> clampFunc;

        protected override Mesh CreateMesh () {
            noiseSources = GetNoiseSources();
            InitNoiseSources(noiseSources);
            clampFunc = GetClampFunc();
            var output = base.CreateMesh();
            output.name = "Custom Terrain";
            return output;
        }

        protected override Vector3 GetAdditionalVertexOffset (Vector3 position) {
            float deformNoise = 0f;
            for(int n=0; n<noiseSources.Length; n++){
                deformNoise += noiseSources[n].Evaluate(position.x, position.z);
            }
            deformNoise += noiseOffsetPre;
            deformNoise = clampFunc(deformNoise);
            deformNoise += nosieOffsetPost;
            return new Vector3(0f, deformNoise * noiseStrength, 0f);
        }

        NoiseSource[] GetNoiseSources () {
            NoiseSource[] output;
            switch(noiseSourceType){
                case NoiseSourceType.None:
                    output = null;
                    break;
                case NoiseSourceType.Perlin: 
                    output = perlinNoiseSources; 
                    break;
                case NoiseSourceType.Texture: 
                    output = textureNoiseSources; 
                    break;
                default: 
                    Debug.LogError($"Unsupported {nameof(NoiseSourceType)} \"{noiseSourceType}\"!");
                    output = null;
                    break;
            }
            if(output == null){
                output = new NoiseSource[0];
            }
            return output;
        }

        void InitNoiseSources (NoiseSource[] input) {
            var rng = GetRNG();
            for(int i=0; i<input.Length; i++){
                input[i].Init(
                    inputOffset: new Vector2(
                        rng.Next(-MAX_RNG_OFFSET, MAX_RNG_OFFSET) + (float)(rng.NextDouble()),
                        rng.Next(-MAX_RNG_OFFSET, MAX_RNG_OFFSET) + (float)(rng.NextDouble())),
                    inputRotation: (float)rng.NextDouble() * Mathf.PI * 2f
                );
            }
        }

        System.Random GetRNG () {
            if(seededRandomness){
                if(terrainSeed == null){
                    terrainSeed = string.Empty;
                }
                return new System.Random(terrainSeed.Trim().GetHashCode());    
            }
            return new System.Random(System.DateTime.Now.GetHashCode());
        }

        System.Func<float, float> GetClampFunc () {
            float min = float.NegativeInfinity;
            float max = float.PositiveInfinity;
            if(clampNoise){
                switch(lowerClamp){
                    case LowerClampValue.MinusOne:
                        min = -1f;
                        break;
                    case LowerClampValue.Zero:
                        min = 0f;
                        break;
                }
                switch(upperClamp){
                    case UpperClampValue.One:
                        max = 1f;
                        break;
                    case UpperClampValue.Zero:
                        max = 0f;
                        break;
                }
            }
            return (f) => {return Mathf.Clamp(f, min, max);};
        }

    }

}