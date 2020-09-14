using UnityEngine;

namespace GeometryGenerators {

    public class TerrainGenerator : PlaneGenerator {

        private const int MAX_RNG_OFFSET = 1024;

        public enum NoiseSourceType {
            NONE,
            PERLIN,
            TEXTURE
        }

        [Header("Deformation Settings")]
        [SerializeField] bool useSeed = false;
        [SerializeField] string seed = string.Empty;
        [SerializeField, Range(-1f, 1f)] float noiseOffset = 0f;
        [SerializeField] float noiseStrength = 1f;
        [SerializeField] NoiseSourceType noiseSourceType = NoiseSourceType.NONE;
        [SerializeField] PerlinNoiseSource[] perlinNoiseSources = default;
        [SerializeField] TextureNoiseSource[] textureNoiseSources = default;

        NoiseSource[] noiseSources;

        protected override Mesh CreateMesh () {
            noiseSources = GetNoiseSources();
            InitNoiseSources(noiseSources); 
            var output = base.CreateMesh();
            output.name = "Custom Terrain";
            return output;
        }

        protected override Vector3 GetAdditionalVertexOffset (Vector3 position) {
            float deformNoise = 0f;
            for(int n=0; n<noiseSources.Length; n++){
                deformNoise += noiseSources[n].Evaluate(position.x, position.z);
            }
            float dY = (deformNoise + noiseOffset) * noiseStrength;
            return new Vector3(0f, dY, 0f);
        }

        NoiseSource[] GetNoiseSources () {
            NoiseSource[] output;
            switch(noiseSourceType){
                case NoiseSourceType.NONE:
                    output = null;
                    break;
                case NoiseSourceType.PERLIN: 
                    output = perlinNoiseSources; 
                    break;
                case NoiseSourceType.TEXTURE: 
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
            if(useSeed){
                if(seed == null){
                    seed = string.Empty;
                }
                return new System.Random(seed.Trim().GetHashCode());    
            }
            return new System.Random(System.DateTime.Now.GetHashCode());
        }

    }

}